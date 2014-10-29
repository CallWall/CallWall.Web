using System.Collections.Generic;

namespace CallWall.Web.Domain
{
    public interface IAccountContactSummary
    {
        bool IsDeleted { get; }

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

        //TODO: Change to be an array/IEnum of avatars/strings.
        /// <summary>
        /// Link to an image or avatar of the contact
        /// </summary>
        string PrimaryAvatar { get; }

        /// <summary>
        /// Any tags a.k.a groups that the contact belongs to. e.g. Friends, Family, Co-workers etc.
        /// </summary>
        IEnumerable<string> Tags { get; }

        /// <summary>
        /// Any identifying handles this contact may have e.g. phone numbers email address user names etc.
        /// </summary>
        IEnumerable<ContactHandle> Handles { get; } 
    }
}