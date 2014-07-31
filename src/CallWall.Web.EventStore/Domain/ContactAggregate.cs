using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CallWall.Web.EventStore.Domain
{

    internal sealed class ContactAggregate : IContactAggregate
    {
        private static int _nextId = 0;
        private readonly List<IContactSummary> _contacts = new List<IContactSummary>();

        public ContactAggregate(IContactSummary root)
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
        public IEnumerable<IContactProviderSummary> Providers { get; private set; }

        public bool OwnsContact(IContactSummary contact)
        {
            return Providers.Any(p => p.ProviderName == contact.Provider
                                      && p.AccountId == contact.AccountId
                                      && p.ContactId == contact.ProviderId);
        }

        public bool IsMatch(IContactSummary contact)
        {
            if (contact == null) throw new ArgumentNullException();
            if (OwnsContact(contact)) throw new InvalidOperationException();

            //Need to apply email/phone/name matching algs here.
            return IsTitleMatch(contact);
        }

        public void Update(IContactSummary contact)
        {
            if (contact == null) throw new ArgumentNullException();
            if (!OwnsContact(contact)) throw new InvalidOperationException();


            //TODO: Compare to previous data and update as appropriate
            //TODO: Remove old data from list, add this to the end of the list (?)

            throw new NotImplementedException();
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

        public void Remove(string provider, string accountId)
        {
            var contactsSnapshot = _contacts.ToList()
                .Where(contact => contact.Provider == provider)
                .Where(contact => contact.AccountId == accountId);
            foreach (var contact in contactsSnapshot)
            {
                _contacts.Remove(contact);
            }
            Refresh();
        }

        public void Add(IContactSummary contact)
        {
            if (contact == null) throw new ArgumentNullException();
            if (OwnsContact(contact)) throw new InvalidOperationException();

            _contacts.Add(contact);
            Refresh();
        }

        public IEnumerable<IContactSummary> Purge()
        {
            var contactSnapshot = _contacts.ToArray();
            _contacts.Clear();
            Refresh();
            return contactSnapshot;
        }

        private bool IsTitleMatch(IContactSummary contact)
        {
            return string.Equals(Title, contact.Title, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Refresh()
        {
            Version++;
            Title = _contacts.Aggregate(string.Empty, (acc, cur) => TitleQuality(acc) >= TitleQuality(cur.Title) ? acc : cur.Title);
            Avatars = _contacts.Select(c => c.PrimaryAvatar).Where(a => a != null).Distinct().ToArray();
            Tags = _contacts.SelectMany(c => c.Tags).Distinct().ToArray();
            Providers = _contacts.Select(c => new ContactProviderSummary(c.Provider,c.AccountId,c.ProviderId)).ToArray();
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
    }
}