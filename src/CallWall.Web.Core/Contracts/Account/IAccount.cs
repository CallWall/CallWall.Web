using System;
using System.Threading.Tasks;

namespace CallWall.Web
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

        ISession CurrentSession { get; }

        Task<User> Login();

        Task RefreshContacts(ContactRefreshTriggers triggeredBy);
    }

    
    public enum ContactRefreshTriggers
    {
        [Obsolete("Default value is not valid", true)]
        None,
        Registered,
        Login,
        UserRequested,
        Timeout
    }
}