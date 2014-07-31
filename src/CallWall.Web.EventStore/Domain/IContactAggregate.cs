using System.Collections.Generic;

namespace CallWall.Web.EventStore.Domain
{
    /// <summary>
    /// Represents an aggregated view of a single contact with data from multiple accounts.
    /// </summary>
    public interface IContactAggregate
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

        bool OwnsContact(IContactSummary contact);
        bool IsMatch(IContactSummary contact);

        void Add(IContactSummary contact);
        void Remove(string provider, string accountId);   
        void Update(IContactSummary contact);
        
        IContactAggregate Merge(IContactAggregate other);
        IEnumerable<IContactSummary> Purge();
    }
}