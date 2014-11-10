using System;
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

        /// <summary>
        /// The Date of birth for the contact. If the Year is unknown then it should be set to a value of 1.
        /// </summary>
        IAnniversary DateOfBirth { get; }

        /// <summary>
        /// The formal or full name of the contact e.g. Mr. Daniel Alex Rowe
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Link to an image or avatar of the contact
        /// </summary>
        IEnumerable<string> AvatarUris { get; }

        /// <summary>
        /// Any tags a.k.a groups that the contact belongs to. e.g. Friends, Family, Co-workers etc.
        /// </summary>
        IEnumerable<string> Tags { get; }

        /// <summary>
        /// Any identifying handles this contact may have e.g. phone numbers email address user names etc.
        /// </summary>
        IEnumerable<ContactHandle> Handles { get; }

        IEnumerable<IContactAssociation> Organizations { get; }

        IEnumerable<IContactAssociation> Relationships { get; }
    }

    public interface IAnniversary
    {
        int? Year { get; }
        int Month { get; }
        int DayOfMonth { get; }
    }

    public class Anniversary : IAnniversary
    {
        private readonly int? _year;
        private readonly int _month;
        private readonly int _dayOfMonth;

        public Anniversary(int year, int month, int dayOfMonth)
        {
            _year = year;
            _month = month;
            _dayOfMonth = dayOfMonth;
        }
        public Anniversary(int month, int dayOfMonth)
        {
            _month = month;
            _dayOfMonth = dayOfMonth;
        }

        public int? Year { get { return _year; } }

        public int Month { get { return _month; } }

        public int DayOfMonth { get { return _dayOfMonth; } }
    }
}