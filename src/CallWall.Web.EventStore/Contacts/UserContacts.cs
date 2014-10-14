using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class UserContacts
    {
        private readonly Guid _userId;
        private readonly List<IContactAggregate> _contacts = new List<IContactAggregate>();
        private readonly List<IContactAggregate> _snapshot = new List<IContactAggregate>();

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
                                         _contacts.Clear();
                                         _contacts.AddRange(_snapshot);
                                         _snapshot.Clear();
                                     });
        }

        public void Add(IAccountContactSummary contact)
        {
            //Look for matches
            var existing = _contacts.SingleOrDefault(c => c.OwnsContact(contact));
            if (existing != null)
            {
                if (contact.IsDeleted)
                {
                    existing.Remove(contact);
                }
                else
                {
                    existing.Update(contact);
                }
                return;
            }

            //This pattern produces different results with different order of input. If A & B can be linked on email, and B & C can be linked on phone, then Adding A, then B, thne C will result in one Aggregate contact (ABC). However adding A then C then B will result in two aggregates (AB & C)
            //var match = _contacts.Select(c => c.Match(contact))
            //    .OrderBy(match => match.Weight)
            //    .FirstOrDefault();

            //TODO: Update to allow for the A,C,B scenario above.
            var match = _contacts.FirstOrDefault(c => c.IsMatch(contact));
            if (match != null)
            {
                match.Add(contact);
                return;
            }

            var newContact = new ContactAggregate(contact);
            _contacts.Add(newContact);
        }

        public ContactAggregateUpdate[] GetChangesSnapshot()
        {
            return _contacts
                .Select(c => c.GetChangesSinceSnapshot())
                .Where(update => update != null)
                .ToArray();
        }

        public void CommitChanges()
        {
            Version++;
            foreach (var contactAggregate in _contacts)
            {
                contactAggregate.CommitChange();
            }
            _snapshot.Clear();
        }
    }
}