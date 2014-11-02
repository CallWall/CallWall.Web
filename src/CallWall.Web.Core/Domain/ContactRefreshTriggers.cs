using System;

namespace CallWall.Web.Domain
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