namespace CallWall.Web.Domain
{
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

        protected bool Equals(ContactProviderSummary other)
        {
            return string.Equals(ProviderName, other.ProviderName) && string.Equals(AccountId, other.AccountId) && string.Equals(ContactId, other.ContactId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContactProviderSummary) obj);
        }

    }
}