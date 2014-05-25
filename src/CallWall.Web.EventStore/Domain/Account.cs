namespace CallWall.Web.EventStore.Domain
{
    //Represents on of potentially many Accounts a user can have. An account is associated to a provider. ie. lee@gmail.com would be an Account with the Gmail provider. A user may have many accounts from the same or different providers.
    public class Account
    {
        

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
            public static readonly string AccountRevoked = "AccountRevoked";

        }
    }
}
