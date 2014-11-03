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


        /// <summary>
        /// The contact handles that are associated to the account. 
        /// </summary>
        IEnumerable<ContactHandle> Handles { get; }


        /// <summary>
        /// The current session information for the account.
        /// </summary>
        ISession CurrentSession { get; }
    }
}