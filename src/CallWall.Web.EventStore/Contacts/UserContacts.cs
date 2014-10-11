using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using CallWall.Web.EventStore.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class UserContacts
    {
        private readonly Guid _userId;
        private readonly List<IContactAggregate> _contacts = new List<IContactAggregate>();
        private readonly List<IContactAggregate> _snapshot = new List<IContactAggregate>();
        private readonly List<ContactAggregateUpdate> _changes = new List<ContactAggregateUpdate>();

        public UserContacts(Guid userId)
        {
            Version = 0;
            _userId = userId;
        }

        public Guid UserId { get { return _userId; } }

        public int Version { get; private set; }

        public IDisposable TrackChanges()
        {
            if (_snapshot.Any())
                throw new InvalidOperationException("Nested Track changes calls are not supported");
            var snapshot = _contacts.Select(c => c.Snapshot());
            _snapshot.AddRange(snapshot);
            return Disposable.Create(() =>
                                     {
                                         //If not committed, then rollback
                                         if (_changes.Any())
                                         {
                                             _contacts.Clear();
                                             _contacts.AddRange(_snapshot);
                                             _snapshot.Clear();
                                             _changes.Clear();
                                         }
                                     });
        }

        public void Add(IAccountContactSummary contact)
        {
            var update = AddContact(contact);
            if(update!=null)
                _changes.Add(update);
        }

        private ContactAggregateUpdate AddContact(IAccountContactSummary contact)
        {
            //Look for matches
            var existing = _contacts.SingleOrDefault(c => c.OwnsContact(contact));
            if (existing != null)
            {
                return contact.IsDeleted 
                    ? existing.Remove(contact) 
                    : existing.Update(contact);
            }
                

            //var match = _contacts.Select(c => c.Match(contact))
            //    .OrderBy(match => match.Weight)
            //    .FirstOrDefault();
            //if (match != null)
            //    return match.Add(contact);

            var newContact = new ContactAggregate(contact);
            _contacts.Add(newContact);
            return new ContactAggregateUpdate
            {
                Id = newContact.Id,
                Version = newContact.Version,
                NewTitle = newContact.Title,
                AddedAvatars = newContact.Avatars.ToArray(),
                AddedProviders = newContact.Providers.ToArray(),
                AddedTags = newContact.Tags.ToArray()
            };
        }

        public ContactAggregateUpdate[] GetChangesSnapshot()
        {
            return _changes.ToArray();
        }

        public void CommitChanges()
        {
            Version += _changes.Count;
            _changes.Clear();
            _snapshot.Clear();
        }
    }
}