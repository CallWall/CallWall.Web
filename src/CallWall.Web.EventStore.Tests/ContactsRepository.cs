using System;
using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore.Tests
{
    public partial class ContactsRepository
    {
        private readonly IEnumerable<IAccountContactProvider> _contactProviders;
        private IEventStoreClient _eventStoreClient;

        public ContactsRepository(IEventStoreClient eventStoreClient, IEnumerable<IAccountContactProvider> contactProviders)
        {
            _contactProviders = contactProviders;
            _eventStoreClient = eventStoreClient;
        }

        public void RefreshContacts(IEnumerable<IAccount> accounts)
        {
            var q = from account in accounts
                    from contactProvider in _contactProviders.Where(cp => account.Provider == cp.Provider).Take(1)
                    select new { account, contactProvider };

            foreach (var pair in q)
            {
                RefreshContacts(pair.account, pair.contactProvider);
            }
        }

        private void RefreshContacts(IAccount account, IAccountContactProvider accountContactProvider)
        {
            throw new System.NotImplementedException();

            //Get or Load the Account's Contact cache/domain object.
            //Call the RequestContactRefresh() method.
            //  this object should decide if a refresh is necessary/acceptable
            //  should load/replay current stream into memory
            //  should log that a refresh was requested, and why it was accepted/rejected
            //  should issue acp.GetContactsFeed(...) 
            //  should receive the ContactsFeed updates and process them
            //  if an Update is appropriate, that should be pushed to the EventStore to cause the in-memory modification.

            //Separately there should be a userContacts domain object/cache. This should be aggregating all the user's Account's contacts.
        }


        //Login
        //Request refresh
        //Write event to AccountContactStream
        //--
        //Another process recives the Refresh request and decides if it should process it or not. If it does, it will make sync calls and add results to ES.
        //--
        //At start up, all AccountStreams will 1st check the head version and then run through to it. Then will enable the fetching of dat from external systems
        //--
        //There will additionally be a userContact stream that is aggregating changes from each account.

        private class RefreshAccountContactsCommand
        {
            public DateTimeOffset TimeStamp { get; set; }
            public string Provider { get; set; }
            public string AccountId { get; set; }
            public string RefreshReason { get; set; }
            public bool WasRefreshAccepted { get; set; }
            public string RefreshDeclinedReason { get; set; }
            public DateTimeOffset NextExpectedEventBy { get; set; }

        }
    }

    /*
     * public abstract class DomainEventBase
     * {
     *      ctor(_conn)
     *      {
     *      
     * }
     * }
     *  
     */


    /*RefreshContactRequested
Decide if appropriate to perform sync
	Log the request with shouldSync flag
	Log next when expected heart beat should occur
	{
		eventType : RefreshRequested
		timeStamp : 2000-12-31T23:59:59.000z
		requestState : Accepted		
		// -or-
		//requestState : Declined
		//requestDeclinedReason : "Synchronization currently running"
		nextExpectedEventBy : 2001-01-01T00:01:29.000z
	}
	
Perform synch
	As batches of contacts are received, they are processed through the model and 
		then added to the event store as a batch update event (with appropriate data e.g. update/insert/delete)
	{
		eventType : contactBatchUpdate
		timeStamp : 2001-01-01T00:00:01.132z
		updates : [
			....
		]
	}
		
	When processing is completed. then a SyncComplete event is logged
	If an error/timeout occurs while synchronizing, then a SyncFailed event is logged
	
Listens to Events
	When an account is logged in (off a registration or a user logging in)
		Then a background task is set up to manage the lifetime of the user's/account's contacts stream

	
Need to update Connection usage/implementation to be a single shared connection*/
}