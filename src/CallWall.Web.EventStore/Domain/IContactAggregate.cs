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
        IEnumerable<ContactProviderSummary> Providers { get; }

        bool OwnsContact(IAccountContactSummary contact);
        bool IsMatch(IAccountContactSummary contact);

        ContactAggregateUpdate Add(IAccountContactSummary contact);
        ContactAggregateUpdate Remove(string provider, string accountId);
        ContactAggregateUpdate Update(IAccountContactSummary contact);

        IContactAggregate Merge(IContactAggregate other);
        IEnumerable<IAccountContactSummary> Purge();

        IContactAggregate Snapshot();
    }
}