using System;
using System.Collections.Generic;

namespace CallWall.Web.Domain
{
    public interface IAccountAuthentication
    {
        IAccountConfiguration Configuration { get; }

        Uri AuthenticationUri(string redirectUri, IList<string> scopes);

        bool CanCreateAccountFromState(string code, string state);

        IAccount CreateAccountFromOAuthCallback(string code, string state);
    }
}