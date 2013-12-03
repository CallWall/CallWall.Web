using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web.Security;

namespace CallWall.Web.Providers
{
    public class SessionProvider : ISessionProvider
    {
        private readonly IEnumerable<IAccountAuthentication> _authenticationProviders;

        public SessionProvider(IEnumerable<IAccountAuthentication> authenticationProviders)
        {
            _authenticationProviders = authenticationProviders;
        }

        public ISession GetSession(IPrincipal user)
        {
            var ident = user.Identity as FormsIdentity;
            return ident != null ? GetSession(ident.Ticket) : null;
        }

        public ISession CreateSession(string code, string state)
        {
            var authProvider = _authenticationProviders.Single(ap => ap.CanCreateSessionFromState(code, state));
            var session = authProvider.CreateSession(code, state);
            return session;
        }

        private ISession GetSession(FormsAuthenticationTicket ticket)
        {
            var sessionPayload = ticket.UserData;
            foreach (var authenticationProvider in _authenticationProviders)
            {
                ISession session;
                if (authenticationProvider.TryDeserialiseSession(sessionPayload, out session))
                {
                    return session;
                }
            }
            return null;
        }
    }
}