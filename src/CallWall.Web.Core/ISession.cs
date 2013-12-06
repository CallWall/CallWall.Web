using System;
using System.Collections.Generic;

namespace CallWall.Web
{
    //TODO: Should this also have an Account property? eg. so we can differentiate between accounts from the same provider?
    //  i.e. Lee@gmail.com & Lee_work@gmail.com
    //TODO: Add username to this account property type too. -LC
    public interface ISession
    {
        string Provider { get; }
        string AccessToken { get; }
        string RefreshToken { get; }
        DateTimeOffset Expires { get; }
        bool HasExpired();
        ISet<string> AuthorizedResources { get; }
        string Serialize();
    }
}