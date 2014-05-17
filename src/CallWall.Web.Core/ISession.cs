﻿using System;
using System.Collections.Generic;
using CallWall.Web.Account;

namespace CallWall.Web
{
    public interface ISession
    {
        string Provider { get; }
        string AccessToken { get; }
        string RefreshToken { get; }
        DateTimeOffset Expires { get; }
        bool HasExpired();
        ISet<string> AuthorizedResources { get; }
        IAccount AccountDetails { get; }
        string Serialize();      
    }
}
