using System;

namespace CallWall.Web
{
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