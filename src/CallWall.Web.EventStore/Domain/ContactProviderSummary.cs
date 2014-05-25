namespace CallWall.Web.EventStore.Domain
{
    internal class ContactProviderSummary : IContactProviderSummary
    {
        private readonly string _providerName;
        private readonly string _accountId;
        private readonly string _contactId;

        public ContactProviderSummary(string providerName, string accountId, string contactId)
        {
            _providerName = providerName;
            _accountId = accountId;
            _contactId = contactId;
        }

        public string ProviderName
        {
            get { return _providerName; }
        }

        public string AccountId
        {
            get { return _accountId; }
        }

        public string ContactId
        {
            get { return _contactId; }
        }
    }
}