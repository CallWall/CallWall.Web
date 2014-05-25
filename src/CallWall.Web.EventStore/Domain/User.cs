namespace CallWall.Web.EventStore.Domain
{
    public class User
    {
        //Login?
        //AccountAdded
        //AccountRemoved        

        public static class EventType
        {
            /// <summary>
            /// Indicates the users has registered an Account with CallWall.
            /// </summary>
            public static readonly string AccountRegistered = "AccountRegistered";

            /// <summary>
            /// Indicates the users used this Account to login to CallWall.
            /// </summary>
            public static readonly string UserLogin = "UserLogin";
        }
    }
}