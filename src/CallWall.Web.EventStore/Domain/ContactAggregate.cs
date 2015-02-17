using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Domain
{
    //TODO: Remove all the safety checking once Unit/Component tests prove the safety (else we will take a massive unnecessary perf hit) -LC
    internal sealed class ContactAggregate : IContactAggregate
    {
        private static int _nextId;
        private readonly List<IAccountContactSummary> _contacts = new List<IAccountContactSummary>();
        private ContactAggregate _snapshot;
        private bool _isDirty;

        public ContactAggregate(IAccountContactSummary root)
        {
            _contacts.Add(root);
            Id = _nextId++;
            Version = 0;
            Refresh();
            _isDirty = true;
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

            return IsEmailMatch(contact)
                || IsPhoneMatch(contact)
                || IsTitleMatch(contact)
                || IsFuzzyTitleMatch(contact);
        }

        public void Add(IAccountContactSummary contact)
        {
#if DEBUG
            if (contact == null) throw new ArgumentNullException();
            if (OwnsContact(contact)) throw new InvalidOperationException();
#endif
            _contacts.Add(contact);
            _isDirty = true;
            Refresh();
        }

        public void Update(IAccountContactSummary newValue)
        {
#if DEBUG
            if (newValue == null) throw new ArgumentNullException();
            if (!OwnsContact(newValue)) throw new InvalidOperationException();
#endif
            var oldValue = _contacts.Single(c => c.Provider == newValue.Provider
                                      && c.AccountId == newValue.AccountId
                                      && c.ProviderId == newValue.ProviderId);
            _contacts.Remove(oldValue);
            _contacts.Add(newValue);
            _isDirty = true;
            Refresh();
        }

        public void Remove(IAccountContactSummary contact)
        {
#if DEBUG
            if (contact == null) throw new ArgumentNullException();
            if (!OwnsContact(contact)) throw new InvalidOperationException();
#endif
            var contactsSnapshot = _contacts.ToList()
                .Where(c => contact.Provider == c.Provider)
                .Where(c => contact.AccountId == c.AccountId)
                .Where(c => contact.ProviderId == c.ProviderId);
            foreach (var c in contactsSnapshot)
            {
                _contacts.Remove(c);
            }

            _isDirty = true;
            Refresh();
        }

        public IContactAggregate Merge(IContactAggregate other)
        {
            throw new NotImplementedException("Yet to prove this method fits the new design");
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
            if (!_isDirty)
                return null;

            var newContactsCount = _contacts.Count;
            if (newContactsCount == 0)
                return new ContactAggregateUpdate
                {
                    Id = Id,
                    Version = Version,
                    IsDeleted = true
                };

            //TODO: Should also manage snapshots around invalid data.
            if (!IsValid())
                return null;

            if (_snapshot == null)
            {
                return new ContactAggregateUpdate
                {
                    Id = Id,
                    Version = Version,
                    NewTitle = Title,
                    AddedAvatars = Avatars.Any() ? Avatars.ToArray() : null,
                    AddedProviders = Providers.Any() ? Providers.ToArray() : null,
                    AddedTags = Tags.Any() ? Tags.ToArray() : null,
                    AddedHandles = Handles.Any() ? Handles.ToArray() : null
                };
            }

            var oldTitle = _snapshot.Title;
            var oldAvatars = _snapshot.Avatars.ToSet();
            var oldTags = _snapshot.Tags.ToSet();
            var oldProviders = _snapshot.Providers.ToSet();
            var oldHandles = _snapshot.Handles.ToSet();

            var avatarDelta = new CollectionDelta<string>(oldAvatars, Avatars);
            var tagDelta = new CollectionDelta<string>(oldTags, Tags);
            var providerDelta = new CollectionDelta<ContactProviderSummary>(oldProviders, Providers);
            var handleDelta = new CollectionDelta<ContactHandle>(oldHandles, Handles);

            var delta = new ContactAggregateUpdate
            {
                Id = Id,
                Version = Version,
                NewTitle = string.Equals(oldTitle, Title, StringComparison.Ordinal) ? null : Title,
                AddedAvatars = avatarDelta.AddedItems,
                RemovedAvatars = avatarDelta.RemovedItems,
                AddedProviders = providerDelta.AddedItems,
                RemovedProviders = providerDelta.RemovedItems,
                AddedTags = tagDelta.AddedItems,
                RemovedTags = tagDelta.RemovedItems,
                AddedHandles = handleDelta.AddedItems,
                RemovedHandles = handleDelta.RemovedItems
            };

            return delta;
        }

        private bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Title);
        }

        public void CommitChange()
        {
            _isDirty = false;
        }

        private void Refresh()
        {
            Version++;
            Providers = _contacts.Select(c => new ContactProviderSummary(c.Provider, c.AccountId, c.ProviderId)).ToArray();
            Avatars = _contacts.SelectMany(c => c.AvatarUris ?? Enumerable.Empty<string>()).Where(a => a != null).Distinct().ToArray();
            Tags = _contacts.SelectMany(c => c.Tags ?? Enumerable.Empty<string>()).Distinct().ToArray();
            //TODO: This may need a custom IComparer instance  -LC
            Handles = _contacts.SelectMany(c => c.Handles ?? Enumerable.Empty<ContactHandle>()).Distinct().ToArray();
            Title = GetBestTitle();
        }

        private string GetBestTitle()
        {
            return GetBestTitle(_contacts.Select(c => !string.IsNullOrWhiteSpace(c.Title)
                ? c.Title
                : GetBestTitle((c.Handles ?? Enumerable.Empty<ContactHandle>()).Select(ch => ch.Handle))));
        }

        private static string GetBestTitle(IEnumerable<string> candidates)
        {
            return candidates.Aggregate((string)null, GetBestTitle);
        }
        private static string GetBestTitle(string a, string b)
        {
            return TitleQuality(a) >= TitleQuality(b)
                ? a
                : b;
        }

        private static int TitleQuality(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (IsEmail(value)) return 1;
            if (IsName(value))
            {
                if (value.IndexOfAny(new[] { ':', ',', ';', '-' }) == -1)
                    return 4;
                return 3;
            }
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


        //TODO: Move algos out to separate set of rules. -LC
        #region Matching Algos

        private bool IsTitleMatch(IAccountContactSummary contact)
        {
            return string.Equals(Title, contact.Title, StringComparison.InvariantCultureIgnoreCase);
        }

        //TODO: Use regex instead?
        private static readonly char[] WordDelimiters = { ' ', ',', ':', ':' };
        private bool IsFuzzyTitleMatch(IAccountContactSummary contact)
        {
            if (Title == null || contact.Title == null)
                return false;
            var titleWords = Title.ToLowerInvariant().Split(WordDelimiters, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x);
            var otherWords = contact.Title.ToLowerInvariant().Split(WordDelimiters, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x);

            return string.Equals(string.Join(",", titleWords.ToArray()), string.Join(",", otherWords.ToArray()));
        }

        private bool IsEmailMatch(IAccountContactSummary contact)
        {
            //Normalize Email Handles
            //Check total count
            //Union the two sets
            //return Union < prior count
            var myEmails = NormalizedEmails(Handles);
            var otherEmails = NormalizedEmails(contact.Handles);

            var sumCount = myEmails.Length + otherEmails.Length;
            return myEmails.Concat(otherEmails).Distinct().Count() < sumCount;
        }

        private static string[] NormalizedEmails(IEnumerable<ContactHandle> contactHandles)
        {
            return (contactHandles ?? Enumerable.Empty<ContactHandle>())
                .Where(h => h.HandleType == ContactHandleTypes.Email)
                .Select(h => NormailizeEmail(h.Handle))
                .Distinct()
                .ToArray();
        }

        private static string NormailizeEmail(string emailAddress)
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
            var filteredPeriods = RemoveLocalPeriods(filterRemoved);
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
        private static string RemoveLocalPeriods(string emailAddress)
        {
            var atSignIndex = emailAddress.IndexOf('@');
            var localPart = emailAddress.Substring(0, atSignIndex);
            var domainPart = emailAddress.Substring(atSignIndex);
            return localPart.Replace(".", string.Empty) + domainPart;            
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
            private readonly HashSet<T> _removedSet;
            private readonly HashSet<T> _addedSet;

            public CollectionDelta(IEnumerable<T> previousState, IEnumerable<T> currentState)
            {
                var previousStateArray = previousState.ToArray();
                _addedSet = currentState.ToSet();

                _removedSet = previousStateArray.ToSet();
                _removedSet.ExceptWith(_addedSet);

                _addedSet.ExceptWith(previousStateArray);
            }

            public T[] RemovedItems { get { return _removedSet.Any() ? _removedSet.ToArray() : null; } }

            public T[] AddedItems { get { return _addedSet.Any() ? _addedSet.ToArray() : null; } }
        }
    }
}