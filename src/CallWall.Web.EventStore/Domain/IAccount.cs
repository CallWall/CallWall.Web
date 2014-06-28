namespace CallWall.Web.EventStore.Domain
{
    public interface IAccount
    {
        string Provider { get; }
        string AccountId { get; }

        //TODO: Merge into the CallWall.Web(.Core).IAccount type. -LC

        ///// <summary>
        ///// The unique username for the account. In many cases this is the user's email address or handle.
        ///// </summary>
        //string Username { get; }

        ///// <summary>
        ///// The name this account displays as the User's name. This may be the user's real name, a nickname or in many cases just the <see cref="Username"/>.
        ///// </summary>
        //string DisplayName { get; }
    }
}