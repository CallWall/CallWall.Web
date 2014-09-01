using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore.Contacts
{
    public sealed class AccountContactsFactory : IAccountContactsFactory
    {
        private readonly IEventStoreConnectionFactory _connectionFactory;
        private readonly IEnumerable<IAccountContactProvider> _accountContactProviders;

        public AccountContactsFactory(IEventStoreConnectionFactory connectionFactory, IEnumerable<IAccountContactProvider> accountContactProviders)
        {
            _connectionFactory = connectionFactory;
            _accountContactProviders = accountContactProviders;
        }

        public AccountContacts Create(IAccountData accountData)
        {
            var accountContactProvider = _accountContactProviders.SingleOrDefault(acp => acp.Provider == accountData.Provider);
            return new AccountContacts(_connectionFactory, accountContactProvider, accountData);
        }
    }
}