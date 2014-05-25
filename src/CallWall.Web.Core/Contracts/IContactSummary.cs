using System.Collections.Generic;

namespace CallWall.Web.Contracts
{
    //TODO: Rename to IAccountContactSummary
    public interface IContactSummary
    {
        /// <summary>
        /// The contact provider 
        /// </summary>
        string Provider { get; }

        /// <summary>
        /// The identifier for the account.
        /// </summary>
        string AccountId { get; }

        //TODO: Rename to ContactSummaryId
        /// <summary>
        /// The provider specific id for the contact
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// How the user commonly references the contact e.g. Dan Rowe
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Link to an image or avatar of the contact
        /// </summary>
        string PrimaryAvatar { get; }

        /// <summary>
        /// Any tags a.k.a groups that the contact belongs to. e.g. Friends, Family, Co-workers etc.
        /// </summary>
        IEnumerable<string> Tags { get; }

    }

    public interface IContactSummaryUpdate
    {
        int Id { get; }
        int Version { get; }

        /// <summary>
        /// How the user commonly references the contact e.g. Dan Rowe
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Any tags a.k.a groups that the contact belongs to. e.g. Friends, Family, Co-workers etc.
        /// </summary>
        IEnumerable<string> Tags { get; }
        /// <summary>
        /// Links to an image or avatar of the contact
        /// </summary>
        IEnumerable<string> Avatars { get; }

        /// <summary>
        /// All of the providers that this contact data is sourced from
        /// </summary>
        IEnumerable<IContactProviderSummary> Providers { get; }
        
    }

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