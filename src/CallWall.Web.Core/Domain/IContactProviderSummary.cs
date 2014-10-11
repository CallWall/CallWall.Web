namespace CallWall.Web.Domain
{
    public interface IContactProviderSummary
    {
        /// <summary>
        /// The contact provider 
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// The specific account for the provider. 
        /// </summary>
        /// <remarks>
        /// A user can potentially have multiple accounts for the same provider e.g. two gmail accounts, a personal and work twitter account etc..
        /// </remarks>
        string AccountId { get; }

        /// <summary>
        /// The provider specific id for the contact
        /// </summary>
        string ContactId { get; }
    }
}