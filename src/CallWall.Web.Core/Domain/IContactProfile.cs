using System;
using System.Collections.Generic;
using System.Linq;

namespace CallWall.Web.Domain
{
    //TODO: Rationalise, I am now going to try and get all of this information at sync time and store locally (at the AccountContact layer). 
    //  Will then need to consider what this means at the UserContact layer.
    public interface IContactProfile
    {
        /// <summary>
        /// How the user commonly references the contact e.g. Dan Rowe
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The formal or full name of the contact e.g. Mr. Daniel Alex Rowe
        /// </summary>
        string FullName { get; }

        //Full Name
        //GivenName
        //FamilyName

        /// <summary>
        /// Link to an image or avatar of the contact
        /// </summary>
        IEnumerable<string> AvatarUris { get; }

        /// <summary>
        /// The Date of birth for the contact. If the Year is unknown then it should be set to a value of 1.
        /// </summary>
        DateTime? DateOfBirth { get; }

        /// <summary>
        /// Any tags a.k.a groups that the contact belongs to. e.g. Friends, Family, Co-workers etc.
        /// </summary>
        IEnumerable<string> Tags { get; }

        IEnumerable<IContactAssociation> Organizations { get; }

        IEnumerable<IContactAssociation> Relationships { get; }

        IEnumerable<ContactHandle> Handles { get; }
    }

    public static class ContactProfileExtensions
    {
        public static string[] ContactKeys(this IContactProfile contactProfile)
        {
            return contactProfile.Handles.Select(h => h.Handle).ToArray();
        }
    }
}