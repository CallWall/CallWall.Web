using System;
using System.Collections.Generic;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleAuthentication : IAccountAuthentication
    {
        public IAccountConfiguration Configuration { get { return FakeGoogleAccountConfiguration.Instance; } }

        public Uri AuthenticationUri(string redirectUri, IList<string> scopes)
        {
            throw new NotImplementedException();
        }

        public ISession CreateSession(string code, string state)
        {
            throw new NotImplementedException();
        }

        public bool TryDeserialiseSession(string payload, out ISession session)
        {
            throw new NotImplementedException();
        }
    }
}