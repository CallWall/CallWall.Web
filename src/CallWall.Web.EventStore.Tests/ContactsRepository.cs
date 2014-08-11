using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore.Tests
{
    public class ContactsRepository
    {
        private readonly IEnumerable<IAccountContactProvider> _contactProviders;
        private EventStore _eventStore;

        public ContactsRepository(IEventStoreConnectionFactory connectionFactory, IEnumerable<IAccountContactProvider> contactProviders)
        {
            _contactProviders = contactProviders;
            _eventStore = new EventStore(connectionFactory);
        }

        public void RefreshContacts(IEnumerable<IAccount> accounts)
        {
            var q = from account in accounts
                    from contactProvider in _contactProviders.Where(cp => account.Provider == cp.Provider).Take(1)
                    select new {account, contactProvider};

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
    }
}