using System;
using System.Collections.Generic;

namespace CallWall.Web
{
    public interface IAccountAuthentication
    {
        IAccountConfiguration Configuration { get; }

        Uri AuthenticationUri(string redirectUri, IList<string> scopes);

        bool CanCreateSessionFromState(string code, string state);
        
        ISession CreateSession(string code, string state);

        bool TryDeserialiseSession(string payload, out ISession session);   
    }
}