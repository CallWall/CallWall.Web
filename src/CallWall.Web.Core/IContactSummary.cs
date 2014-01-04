using System.Collections.Generic;

namespace CallWall.Web
{
    public interface IContactSummary
    {
        /// <summary>
        /// The provider specific id for the contact
        /// </summary>
        string Id { get; }
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
}