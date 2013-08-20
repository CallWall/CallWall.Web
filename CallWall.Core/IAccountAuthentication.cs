using System;
using System.Collections.Generic;

namespace CallWall
{
    public interface IAccountAuthentication
    {
        IAccountConfiguration Configuration { get; }

        Uri AuthenticationUri(string redirectUri, IList<string> scopes);

        ISession CreateSession(string code, string state);
    }
}