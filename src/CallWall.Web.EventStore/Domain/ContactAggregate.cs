using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Domain
{
    //TODO: Remove all the safety checking once Unit/COmponent tests prove the safety (else we will take a massive unnecessary perf hit) -LC
    internal sealed class ContactAggregate : IContactAggregate
    {
        private static int _nextId = 0;
        private readonly List<IAccountContactSummary> _contacts = new List<IAccountContactSummary>();
        private ContactAggregate _snapshot;

        public ContactAggregate(IAccountContactSummary root)
        {
            _contacts.Add(root);
            Id = _nextId++;
            Version = 0;
            Refresh();
        }

        public int Id { get; private set; }
        public int Version { get; private set; }

        /// <summary>
        /// How the user commonly references the contact e.g. Dan Rowe
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Any tags a.k.a groups that the contact belongs to. e.g. Friends, Family, Co-workers etc.
        /// </summary>
        public IEnumerable<string> Tags { get; private set; }
        /// <summary>
        /// Links to an image or avatar of the contact
        /// </summary>
        public IEnumerable<string> Avatars { get; private set; }

        public IEnumerable<ContactHandle> Handles { get; private set; }

        /// <summary>
        /// All of the providers that this contact data is sourced from
        /// </summary>
        public IEnumerable<ContactProviderSummary> Providers { get; private set; }

        public bool OwnsContact(IAccountContactSummary contact)
        {
            return Providers.Any(p => p.ProviderName == contact.Provider
                                      && p.AccountId == contact.AccountId
                                      && p.ContactId == contact.ProviderId);
        }

        public bool IsMatch(IAccountContactSummary contact)
        {
            if (contact == null) throw new ArgumentNullException();
            if (OwnsContact(contact)) throw new InvalidOperationException();

            //Need to apply email/phone/name matching algs here.
            return IsEmailMatch(contact)
                || IsPhoneMatch(contact)
                   || IsTitleMatch(contact);

        }

        public void Add(IAccountContactSummary contact)
        {
            if (contact == null) throw new ArgumentNullException();
            if (OwnsContact(contact)) throw new InvalidOperationException();

            //return CreateDelta(() => _contacts.Add(contact));
            _contacts.Add(contact);
        }

        public void Update(IAccountContactSummary newValue)
        {
            if (newValue == null) throw new ArgumentNullException();
            if (!OwnsContact(newValue)) throw new InvalidOperationException();

            var oldValue = _contacts.Single(c => c.Provider == newValue.Provider
                                      && c.AccountId == newValue.AccountId
                                      && c.ProviderId == newValue.ProviderId);
            _contacts.Remove(oldValue);
            _contacts.Add(newValue);
        }

        public void Remove(IAccountContactSummary contact)
        {
            if (contact == null) throw new ArgumentNullException();
            if (!OwnsContact(contact)) throw new InvalidOperationException();

            Action removal = () =>
            {
                var contactsSnapshot = _contacts.ToList()
                    .Where(c => contact.Provider == c.Provider)
                    .Where(c => contact.AccountId == c.AccountId)
                    .Where(c => contact.ProviderId == c.ProviderId);
                foreach (var c in contactsSnapshot)
                {
                    _contacts.Remove(c);
                }
            };
            //return CreateDelta(removal);
            removal();
        }

        public IContactAggregate Merge(IContactAggregate other)
        {
            var otherContacts = other.Purge();
            foreach (var otherContact in otherContacts)
            {
                Add(otherContact);
            }

            return this;
        }

        public IEnumerable<IAccountContactSummary> Purge()
        {
            var contactSnapshot = _contacts.ToArray();
            _contacts.Clear();
            Refresh();
            return contactSnapshot;
        }

        public IContactAggregate Snapshot()
        {
            var copy = new ContactAggregate(_contacts[0]);
            foreach (var contact in _contacts.Skip(1))
            {
                copy.Add(contact);
            }
            _snapshot = copy;
            return copy;
        }

        public ContactAggregateUpdate GetChangesSinceSnapshot()
        {
            if (_snapshot == null)
            {
                Refresh();
                return new ContactAggregateUpdate
                {
                    Id = this.Id,
                    Version = this.Version,
                    NewTitle = this.Title,
                    AddedAvatars = this.Avatars.ToArray(),
                    AddedProviders = this.Providers.ToArray(),
                    AddedTags = this.Tags.ToArray(),
                    AddedHandles = this.Handles.ToArray()
                };
            }


            var oldTitle = _snapshot.Title;
            var oldAvatars = _snapshot.Avatars.ToSet();
            var oldTags = _snapshot.Tags.ToSet();
            var oldProviders = _snapshot.Providers.ToSet();

            
            Refresh();

            var newContactsCount = _contacts.Count;

            if (newContactsCount == 0)
                return new ContactAggregateUpdate
                {
                    Id = this.Id,
                    Version = this.Version,
                    IsDeleted = true
                };


            var avatarDelta = new CollectionDelta<string>(oldAvatars, Avatars);
            var tagDelta = new CollectionDelta<string>(oldTags, Tags);
            var providerDelta = new CollectionDelta<ContactProviderSummary>(oldProviders, Providers);

            //TODO: If the result is no change, then return null

            var delta = new ContactAggregateUpdate
            {
                Id = this.Id,
                Version = this.Version,
                NewTitle = string.Equals(oldTitle, Title, StringComparison.Ordinal) ? null : Title,
                AddedAvatars = avatarDelta.AddedItems.ToArray(),
                RemovedAvatars = avatarDelta.RemovedItems.ToArray(),
                AddedProviders = providerDelta.AddedItems.ToArray(),
                RemovedProviders = providerDelta.RemovedItems.ToArray(),
                AddedTags = tagDelta.AddedItems.ToArray(),
                RemovedTags = tagDelta.RemovedItems.ToArray(),
            };

            return delta;
        }


        private void Refresh()
        {
            Version++;
            Title = _contacts.Aggregate(string.Empty, (acc, cur) => TitleQuality(acc) >= TitleQuality(cur.Title) ? acc : cur.Title);
            Avatars = _contacts.Select(c => c.PrimaryAvatar).Where(a => a != null).Distinct().ToArray();
            Tags = _contacts.SelectMany(c => c.Tags).Distinct().ToArray();
            //TODO: This may need a custom IComparer instance  -LC
            Handles = _contacts.SelectMany(c => c.Handles).Distinct().ToArray();
            Providers = _contacts.Select(c => new ContactProviderSummary(c.Provider, c.AccountId, c.ProviderId)).ToArray();
        }

        private static int TitleQuality(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (IsEmail(value)) return 1;
            if (IsName(value)) return 3;
            return 2;
        }

        private static bool IsEmail(string value)
        {
            var regex = new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b");
            return regex.IsMatch(value);
        }

        private static bool IsName(string value)
        {
            var regex = new Regex(@"([A-Z]\w*\.?\s+)*([A-Z]\w*)");
            return regex.IsMatch(value);
        }

        private ContactAggregateUpdate CreateDelta(Action mutation)
        {
            var oldTitle = Title;
            var oldAvatars = Avatars.ToSet();
            var oldTags = Tags.ToSet();
            var oldProviders = Providers.ToSet();

            mutation();
            Refresh();

            var newContactsCount = _contacts.Count;

            if (newContactsCount == 0)
                return new ContactAggregateUpdate
                {
                    Id = this.Id,
                    Version = this.Version,
                    IsDeleted = true
                };


            var avatarDelta = new CollectionDelta<string>(oldAvatars, Avatars);
            var tagDelta = new CollectionDelta<string>(oldTags, Tags);
            var providerDelta = new CollectionDelta<ContactProviderSummary>(oldProviders, Providers);

            //TODO: If the result is no change, then return null

            var delta = new ContactAggregateUpdate
            {
                Id = this.Id,
                Version = this.Version,
                NewTitle = string.Equals(oldTitle, Title, StringComparison.Ordinal) ? null : Title,
                AddedAvatars = avatarDelta.AddedItems.ToArray(),
                RemovedAvatars = avatarDelta.RemovedItems.ToArray(),
                AddedProviders = providerDelta.AddedItems.ToArray(),
                RemovedProviders = providerDelta.RemovedItems.ToArray(),
                AddedTags = tagDelta.AddedItems.ToArray(),
                RemovedTags = tagDelta.RemovedItems.ToArray(),
            };

            return delta;
        }
        

        #region Matching Algos

        private bool IsTitleMatch(IAccountContactSummary contact)
        {
            return string.Equals(Title, contact.Title, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool IsEmailMatch(IAccountContactSummary contact)
        {
            //Normalize Email Handles
            //Check total count
            //Union the two sets
            //return Union < prior count
            var myEmails = Handles.Where(h => h.HandleType == ContactHandleTypes.Email)
                .Select(h => NormailizeEmail(h.Handle))
                .ToArray();
            var otherEmails = contact.Handles.Where(h => h.HandleType == ContactHandleTypes.Email)
                .Select(h => NormailizeEmail(h.Handle))
                .ToArray();

            var sumCount = myEmails.Length + otherEmails.Length;
            return myEmails.Concat(otherEmails).Distinct().Count() < sumCount;
        }

        private string NormailizeEmail(string emailAddress)
        {
            var lowerCased = emailAddress.ToLowerInvariant();
            if (IsGmail(lowerCased))
            {
                return StripForGmail(lowerCased);
            }
            return lowerCased;
        }

        private static string StripForGmail(string lowerCased)
        {
            //replace googlemail.com with gmail.com
            //remove any thing between + and @
            //strip all .

            var standardisedDomain = lowerCased.Replace("@googlemail.com", "@gmail.com");
            var filterRemoved = RemoveFilterSuffix(standardisedDomain);
            var filteredPeriods = filterRemoved.Replace(".", string.Empty);
            return filteredPeriods;
        }

        private static string RemoveFilterSuffix(string emailAddress)
        {
            var plusSignIdx = emailAddress.IndexOf('+');
            if (plusSignIdx != -1)
            {
                var atSignIndex = emailAddress.IndexOf('@');
                return emailAddress.Remove(plusSignIdx, atSignIndex - plusSignIdx);
            }
            return emailAddress;
        }

        private static bool IsGmail(string lowerCased)
        {
            return lowerCased.EndsWith("@gmail.com", StringComparison.Ordinal)
                   || lowerCased.EndsWith("@googlemail.com", StringComparison.Ordinal);
        }

        private bool IsPhoneMatch(IAccountContactSummary contact)
        {
            return false;
        }
        #endregion

        private sealed class CollectionDelta<T>
        {
            private readonly HashSet<T> _removedItems;
            private readonly HashSet<T> _addedItems;

            public CollectionDelta(IEnumerable<T> previousState, IEnumerable<T> currentState)
            {
                var previousStateArray = previousState.ToArray();
                _addedItems = currentState.ToSet();

                var removedItems = previousStateArray.ToSet();
                _removedItems = removedItems;
                RemovedItems.ExceptWith(AddedItems);

                AddedItems.ExceptWith(previousStateArray);
            }

            public HashSet<T> RemovedItems { get { return _removedItems; } }

            public HashSet<T> AddedItems { get { return _addedItems; } }
        }
    }
}