using System;
using System.Collections.Generic;

namespace CallWall.Web
{
    public interface IAccountAuthentication
    {
        IAccountConfiguration Configuration { get; }

        Uri AuthenticationUri(string redirectUri, IList<string> scopes);

        bool CanCreateAccountFromState(string code, string state);

        IAccount CreateAccountFromOAuthCallback(string code, string state);

        //TODO: Is this required anymore? -LC
        bool TryDeserialiseSession(string payload, out ISession session);   
    }
}