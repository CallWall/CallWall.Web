using System.Collections.Generic;

namespace CallWall.Web
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

        /// <summary>
        /// Link to an image or avatar of the contact
        /// </summary>
        string PrimaryAvatar { get; }

        /// <summary>
        /// Any tags a.k.a groups that the contact belongs to. e.g. Friends, Family, Co-workers etc.
        /// </summary>
        IEnumerable<string> Tags { get; }

        ///// <summary>
        ///// Any identifying handles this contact may have e.g. phone numbers email address user names etc.
        ///// </summary>
        //IEnumerable<IContactHandle> Handles { get; } 
    }

    public abstract class ContactHandleTypes
    {
        public static readonly string Phone = "Phone";
        public static readonly string Email = "Email";
    }

    public interface IContactHandle
    {
        string HandleType { get; }
        string Handle { get; }
    }
}