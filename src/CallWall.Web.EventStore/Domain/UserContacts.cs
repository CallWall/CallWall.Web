using System;
using System.Collections.Generic;

namespace CallWall.Web.EventStore.Domain
{
    public class UserContacts
    {
        private readonly IEventStore _eventStore;
        private readonly int _userId;
        private readonly List<IAccountContacts> _accountContacts = new List<IAccountContacts>(); 
        //ContactsRefreshRequested
        //ContactsRefreshCompleted
        //ContactAggregatesUpdated  --Only needs one event. ver1 means add, and Contacts.length==0 is deleted/tombstoned.
        //                          --Migrations need to be a single event, else we will break some invariants (or just have inconsistent state for some given events)

        public UserContacts(IEventStore eventStore, int userId)
        {
            _eventStore = eventStore;
            _userId = userId;
        }

        public void AddAccount(string provider, string accountId)
        {
            throw new NotImplementedException();
        }

        public void RemoveAccount(string provider, string accountId)
        {
            throw new NotImplementedException();
        }

        public void RequestRefresh()
        {
            foreach (var account in _accountContacts)
            {
                account.RequestRefresh();
            }
        }

        public IObservable<ContactAggregate> GetContacts(int fromEventId)
        {
            throw new NotImplementedException();
        }


        public static string StreamName(int userId)
        {
            return string.Format(@"UserContacts-{0}", userId);
        }
        public static class EventType
        {
            public static readonly string AccountAdded = "AccountAdded";
            public static readonly string AccountRemoved = "AccountRemoved";
            /// <summary>
            /// Represents an event that is an atomic update of one or more ContactAggregates.
            /// </summary>
            /// <remarks>
            /// If the addition of a contact summary from an account results in a linking of two (or more) existing ContactAggregates, 
            /// then both aggregates will be updated in the same transaction.
            /// If a ContactAggregate has all of its ContactSummaries removed (either due to a merge or the account being removed), the 
            /// ContactAggregate will be saved as empty and thus become a tombstone.
            /// </remarks>
            public static readonly string ContactAggregatesUpdated = "ContactAggregatesUpdated";
        }
    }
}