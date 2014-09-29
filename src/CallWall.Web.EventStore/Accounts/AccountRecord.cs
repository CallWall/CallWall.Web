namespace CallWall.Web.EventStore.Accounts
{
    public class AccountRecord : IAccount
    {
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public SessionRecord CurrentSession { get; set; }

        public override string ToString()
        {
            return string.Format("Provider: {0}, AccountId: {1}, DisplayName: {2}, CurrentSession: {3}", Provider, AccountId,
                DisplayName, CurrentSession);
        }

        string IAccount.Provider { get { return Provider; } }
        string IAccount.AccountId { get { return AccountId; } }
        string IAccount.DisplayName { get { return DisplayName; } }
        ISession IAccount.CurrentSession { get { return CurrentSession; } }
    }
}