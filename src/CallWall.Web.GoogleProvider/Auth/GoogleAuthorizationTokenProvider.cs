using System;
using System.Collections.Generic;
using System.Threading;
using CallWall.Web.Contracts;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Auth
{
    public class GoogleAuthorizationTokenProvider : IAuthorizationTokenProvider
    {


        public IEnumerable<string> RequestAccessToken(IResourceScope scope)
        {
            throw new NotSupportedException("Make this obsolete");

            //Completely untested stab in the dark code - rc
            //return _sessionProvider.GetSessions(Thread.CurrentPrincipal)
            //    .Where(s => s.Provider == "Google")
            //    .Where(s=>s.AuthorizedResources.Contains(scope.Resource))
            //    .Select(s=>s.AccessToken);
        }
    }
}