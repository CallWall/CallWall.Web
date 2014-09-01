namespace CallWall.Web.EventStore.Domain
{
    //Not sure if the interface is needed or even helpful -LC
    public class ContactProviderSummary : IContactProviderSummary
    {
        public ContactProviderSummary()
        {
        }

        public ContactProviderSummary(string providerName, string accountId, string contactId)
        {
            ProviderName = providerName;
            AccountId = accountId;
            ContactId = contactId;
        }

        public string ProviderName { get; set; }

        public string AccountId { get; set; }

        public string ContactId { get; set; }
    }
}