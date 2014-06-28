using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CallWall.Web.EventStore.Domain
{
    internal class UserContacts : IUserContacts
    {
        private readonly IEventStore _eventStore;
        private readonly int _userId;
        private readonly List<IAccountContacts> _accountContacts = new List<IAccountContacts>();
        private readonly string _streamName;
        

        public UserContacts(IEventStore eventStore, int userId)
        {
            _eventStore = eventStore;
            _userId = userId;
            _streamName = StreamName(userId);
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

        public IObservable<IContactAggregate> GetContacts(int fromEventId)
        {
            return _eventStore.GetEvents(_streamName, fromEventId)
                              .Where(re=>re.Event.EventType == EventType.ContactAggregatesUpdated)
                              //TODO: Introduce a way to rehydrate ContactAggregates from JSON
                              .Select(re => re.Event.Data)
                              .Select(Encoding.UTF8.GetString)
                              .Select(JsonConvert.DeserializeObject<ContactAggregate>);
        }


        public static string StreamName(int userId)
        {
            return string.Format(@"UserContacts-{0}", userId);
        }
        public static class EventType
        {
            //ContactsRefreshRequested
            //ContactsRefreshCompleted
            //ContactAggregatesUpdated  --Only needs one event. ver1 means add, and Contacts.length==0 is deleted/tombstoned.
            //                          --Migrations need to be a single event, else we will break some invariants (or just have inconsistent state for some given events)

            public static readonly string AccountAdded = "AccountAdded";    //Could be a linked event
            public static readonly string AccountRemoved = "AccountRemoved";    //Could be a linked event to Account.EventType.AccountDeregistered and Account.EventType.AccountRevoked.
            
            public static readonly string ContactsRefreshRequested = "ContactsRefreshRequested";
            public static readonly string ContactsRefreshCompleted = "ContactsRefreshCompleted";    



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