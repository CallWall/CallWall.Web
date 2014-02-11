using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CallWall.Web.Contracts;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider.Auth
{
    public class GoogleAuthorizationTokenProvider : IAuthorizationTokenProvider
    {
        private readonly ISessionProvider _sessionProvider;

        public GoogleAuthorizationTokenProvider(ISessionProvider sessionProvider)
        {
            _sessionProvider = sessionProvider;
        }

        public IEnumerable<string> RequestAccessToken(IResourceScope scope)
        {
            //Completely untested stab in the dark code - rc
            return _sessionProvider.GetSessions(Thread.CurrentPrincipal)
                .Where(s => s.Provider == "Google")
                .Where(s=>s.AuthorizedResources.Contains(scope.Resource))
                .Select(s=>s.AccessToken);
        }
    }
}