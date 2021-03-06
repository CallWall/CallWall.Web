using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore.Contacts
{
    public sealed class AccountContactsFactory : IAccountContactsFactory
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IReadOnlyDictionary<string, IAccountContactProvider> _accountContactProviders;

        public AccountContactsFactory(IEventStoreClient eventStoreClient, ILoggerFactory loggerFactory, IEnumerable<IAccountContactProvider> accountContactProviders)
        {
            _eventStoreClient = eventStoreClient;
            _loggerFactory = loggerFactory;

            _accountContactProviders = accountContactProviders.ToReadOnlyDictionary(acp => acp.Provider);
        }

        public AccountContacts Create(IAccount account)
        {
            var accountContactProvider = _accountContactProviders[account.Provider];
            return new AccountContacts(_eventStoreClient, _loggerFactory, accountContactProvider, account);
        }
    }
}