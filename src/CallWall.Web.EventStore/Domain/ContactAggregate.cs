using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace CallWall.Web.EventStore.Domain
{

    internal sealed class ContactAggregate : IContactAggregate
    {
        private static int _nextId = 0;
        private readonly List<IAccountContactSummary> _contacts = new List<IAccountContactSummary>();

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
            return IsTitleMatch(contact);
        }

        public ContactAggregateUpdate Add(IAccountContactSummary contact)
        {
            if (contact == null) throw new ArgumentNullException();
            if (OwnsContact(contact)) throw new InvalidOperationException();

            return CreateDelta(() => _contacts.Add(contact));
        }

        public ContactAggregateUpdate Update(IAccountContactSummary contact)
        {
            if (contact == null) throw new ArgumentNullException();
            if (!OwnsContact(contact)) throw new InvalidOperationException();


            //TODO: Compare to previous data and update as appropriate
            //TODO: Remove old data from list, add this to the end of the list (?)

            throw new NotImplementedException();
        }

        public ContactAggregateUpdate Remove(IAccountContactSummary contact)
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
            return CreateDelta(removal);
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
            return copy;
        }

        private bool IsTitleMatch(IAccountContactSummary contact)
        {
            return string.Equals(Title, contact.Title, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Refresh()
        {
            Version++;
            Title = _contacts.Aggregate(string.Empty, (acc, cur) => TitleQuality(acc) >= TitleQuality(cur.Title) ? acc : cur.Title);
            Avatars = _contacts.Select(c => c.PrimaryAvatar).Where(a => a != null).Distinct().ToArray();
            Tags = _contacts.SelectMany(c => c.Tags).Distinct().ToArray();
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