namespace CallWall.Web.EventStore.Accounts
{
    internal static class AccountEventType
    {
        public const string AccountRegististered = "AccountRegistered";

        /// <summary>
        /// Indicates that this Account was used to login to CallWall.
        /// </summary>
        public const string AccountLogin = "AccountLogin";

        /// <summary>
        /// Indicates that the user has removed this account from CallWall
        /// </summary>
        public const string AccountDeregistered = "AccountDeregistered";

        /// <summary>
        /// Indicates that the Provider or User has revoked access from CallWall accessing this account.
        /// </summary>
        public const string AccountRevoked = "AccountRevoked";
        //May have to be a linked event from a AccountContactSummaryRefresh failure. -LC
    }
}