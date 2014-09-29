using System;

namespace CallWall.Web.EventStore.Accounts
{
    //Was usfull when the Account object had methods that delegated to Repos. Maybe not needed now. -LC
    public class AccountFactory : IAccountFactory
    {
        public IAccount Create(string accountId, string provider, string displayName, ISession session)
        {
            if (string.IsNullOrWhiteSpace(accountId)) throw new ArgumentException("Parameter may not be null or blank", "accountId");
            if (string.IsNullOrWhiteSpace(provider)) throw new ArgumentException("Parameter may not be null or blank", "provider");
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Parameter may not be null or blank", "displayName");
            if (session == null) throw new ArgumentNullException("session");

            return new Account
            {
                AccountId = accountId,
                Provider = provider,
                DisplayName = displayName,
                CurrentSession = new Session(
                    session.AccessToken,
                    session.RefreshToken,
                    session.Expires,
                    session.AuthorizedResources)
            };
        }
    }
}