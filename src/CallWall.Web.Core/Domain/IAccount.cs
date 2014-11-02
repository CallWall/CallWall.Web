using System.Collections.Generic;

namespace CallWall.Web.Domain
{
    public interface IAccount
    {
        /// <summary>
        /// The provider for which this account belongs to
        /// </summary>
        string Provider { get; }

        /// <summary>
        /// The unique username for the account. In many cases this is the user's email address or handle.
        /// </summary>
        string AccountId { get; }

        /// <summary>
        /// The name this account displays as the User's name. This may be the user's real name, a nickname or in many cases just the <see cref="AccountId"/>.
        /// </summary>
        string DisplayName { get; }

        IEnumerable<ContactHandle> Handles { get; }

        ISession CurrentSession { get; }
    }
}