using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.Providers
{
    public class SessionProvider : ISessionProvider
    {
        private readonly IEnumerable<IAccountAuthentication> _authenticationProviders;

        public SessionProvider(IEnumerable<IAccountAuthentication> authenticationProviders)
        {
            _authenticationProviders = authenticationProviders;
        }

        public IEnumerable<ISession> GetSessions(IPrincipal user)
        {
            var ident = user.Identity as FormsIdentity;
            return ident != null ? GetSessions(ident.Ticket) : Enumerable.Empty<ISession>();
        }

        public ISession CreateSession(string code, string state)
        {
            var authProvider = _authenticationProviders.Single(ap => ap.CanCreateSessionFromState(code, state));
            var session = authProvider.CreateSession(code, state);
            return session;
        }

        public void SetPrincipal(Controller controller, ISession session)
        {
            var sessions = GetSessions(controller.Request.Cookies).ToDictionary(x => x.Provider);
            sessions.Add(session.Provider, session);
            var jObject = JObject.FromObject(sessions);
            var json = jObject.ToString(Formatting.None);
            var authTicket = new FormsAuthenticationTicket(1, session.AccountDetails.DisplayName, DateTime.UtcNow, DateTime.MaxValue, true, json, "CallWallAuth");
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            controller.Response.Cookies.Add(faCookie);
        }

        public void LogOff()
        {
            FormsAuthentication.SignOut();
        }
        
        private IEnumerable<ISession> GetSessions(HttpCookieCollection cookies)
        {
            if (!FormsAuthentication.CookiesSupported) return null;
            var authCookie = cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                return authTicket == null ?
                    Enumerable.Empty<ISession>()
                    : GetSessions(authTicket);
            }
            return Enumerable.Empty<ISession>();
        }

        private IEnumerable<ISession> GetSessions(FormsAuthenticationTicket ticket)
        {
            var sessionPayload = ticket.UserData;
            foreach (var authenticationProvider in _authenticationProviders)
            {
                ISession session;
                if (authenticationProvider.TryDeserialiseSession(sessionPayload, out session))
                {
                    yield return session;
                }
            }
        }
    }
}
