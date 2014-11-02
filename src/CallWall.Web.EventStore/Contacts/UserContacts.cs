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
        private readonly List<IContactAggregate> _rollbackSnapshot = new List<IContactAggregate>();
        private ContactAggregateUpdate[] _changesSnapshot;
        private bool _isTrackingChanges;
        private bool _isCommited;

        public UserContacts(Guid userId)
        {
            Version = 0;
            _userId = userId;
        }

        public Guid UserId { get { return _userId; } }

        public int Version { get; private set; }

        public IDisposable TrackChanges()
        {
            if (_isTrackingChanges) throw new InvalidOperationException("Nested Track changes calls are not supported");

            _isTrackingChanges = true;
            _isCommited = false;
            var snapshot = _contacts.Select(c => c.Snapshot());
            _rollbackSnapshot.AddRange(snapshot);
            return Disposable.Create(() =>
                                     {
                                         if (!_isCommited)
                                         {
                                             _contacts.Clear();
                                             _contacts.AddRange(_rollbackSnapshot);
                                             _rollbackSnapshot.Clear();    
                                         }
                                         _changesSnapshot = null;
                                         _isTrackingChanges = false;
                                     });
        }

        public void Add(IAccountContactSummary contact)
        {
            if (!_isTrackingChanges) throw new InvalidOperationException("Add must be called while tracking changes");

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
            if (!_isTrackingChanges) throw new InvalidOperationException("Must be TrackingChanges() to be able to get a snapshot.");
            if (_changesSnapshot != null) throw new InvalidOperationException("Nested snapshots not supported. Either CommitChanges() or rollback by disposing the TrackChanges() token.");
            _changesSnapshot = _contacts
                .Select(c => c.GetChangesSinceSnapshot())
                .Where(update => update != null)
                .ToArray();
            return _changesSnapshot;
        }

        public void CommitChanges()
        {
            _isCommited = true;
            Version += _changesSnapshot.Length;
            foreach (var contactAggregate in _contacts)
            {
                contactAggregate.CommitChange();
            }
            _rollbackSnapshot.Clear();
        }
    }
}