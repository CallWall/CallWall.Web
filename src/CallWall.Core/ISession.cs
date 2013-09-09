using System;
using System.Collections.Generic;

namespace CallWall
{
    public interface ISession
    {
        string Provider { get; }
        string AccessToken { get; }
        string RefreshToken { get; }
        DateTimeOffset Expires { get; }
        bool HasExpired();
        ISet<Uri> AuthorizedResources { get; }
    }
}