using System;
using System.Collections.Generic;
using System.Linq;
using CallWall.Web.EventStore.Accounts;

namespace CallWall.Web.Domain
{
    //Was useful when the Account object had methods that delegated to Repos. Maybe not needed now. -LC
    public class AccountFactory : IAccountFactory
    {
        public IAccount Create(string accountId, string provider, string displayName, ISession session, IEnumerable<ContactHandle> contactHandles)
        {
            if (string.IsNullOrWhiteSpace(accountId)) throw new ArgumentException("Parameter may not be null or blank", "accountId");
            if (string.IsNullOrWhiteSpace(provider)) throw new ArgumentException("Parameter may not be null or blank", "provider");
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Parameter may not be null or blank", "displayName");
            if (contactHandles == null) throw new ArgumentNullException("contactHandles", "Parameter may not be null");
            if (session == null) throw new ArgumentNullException("session");

            return new Account
            {
                AccountId = accountId,
                Provider = provider,
                DisplayName = displayName,
                Handles = contactHandles.ToArray(),
                CurrentSession = new Session(
                    session.AccessToken,
                    session.RefreshToken,
                    session.Expires,
                    session.AuthorizedResources)
            };
        }
    }
}