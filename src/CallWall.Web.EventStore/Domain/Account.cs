using System;

namespace CallWall.Web.EventStore.Domain
{
    //Represents on of potentially many Accounts a user can have. An account is associated to a provider. ie. lee@gmail.com would be an Account with the Gmail provider. A user may have many accounts from the same or different providers.
    internal class Account : IAccount
    {
        private readonly string _provider;
        private readonly string _accountId;

        public Account(string providerName, string accountId) //, IAccountProvider/AccountAuthProvider accountProvider)
        {
            _provider = providerName;
            _accountId = accountId;
        }
        
        public string Provider { get { return _provider; } }

        public string AccountId { get { return _accountId; } }

        public void Deregister()
        {
            throw new NotImplementedException();
        }


        public static string StreamName(string providerName, string accountId)
        {
            return string.Format(@"Account-{0}-{1}", providerName, accountId);
        }
        public static class EventType
        {
            /// <summary>
            /// Indicates the users used this Account to login to CallWall.
            /// </summary>
            public static readonly string AccountLogin = "AccountLogin";

            /// <summary>
            /// Indicates that the user has removed this account from CallWall
            /// </summary>
            public static readonly string AccountDeregistered = "AccountDeregistered";      

            /// <summary>
            /// Indicates that the Provider or User has revoked access from CallWall accessing this account.
            /// </summary>
            public static readonly string AccountRevoked = "AccountRevoked";                //May have to be a linked event from a AccountContactSummaryRefresh failure. -LC

        }

    }
}
