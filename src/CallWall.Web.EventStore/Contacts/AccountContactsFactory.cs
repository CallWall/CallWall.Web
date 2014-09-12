using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore.Contacts
{
    public sealed class AccountContactsFactory : IAccountContactsFactory
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly IEnumerable<IAccountContactProvider> _accountContactProviders;

        public AccountContactsFactory(IEventStoreClient eventStoreClient, IEnumerable<IAccountContactProvider> accountContactProviders)
        {
            _eventStoreClient = eventStoreClient;
            _accountContactProviders = accountContactProviders;
        }

        public AccountContacts Create(IAccountData accountData)
        {
            var accountContactProvider = _accountContactProviders.SingleOrDefault(acp => acp.Provider == accountData.Provider);
            return new AccountContacts(_eventStoreClient, accountContactProvider, accountData);
        }
    }
}