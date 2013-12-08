using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.Providers
{
    public class PrincipalManager : IManagePrincipal
    {
        internal const string ProviderTypeKey = "http://callwall.com/identity/Provider";
        internal const string AccessTokenTypeKey = "http://callwall.com/identity/AccessToken";
        internal const string RefreshTokenTypeKey = "http://callwall.com/identity/RefreshToken";
        internal const string ExpiryTypeKey = "http://callwall.com/identity/Expires";
        internal const string ResourceTypeKey = "http://callwall.com/identity/Resource";

        private readonly IEnumerable<IAccountAuthentication> _authenticationProviders;

        public PrincipalManager(IEnumerable<IAccountAuthentication> authenticationProviders)
        {
            _authenticationProviders = authenticationProviders;
        }

        public IPrincipal GetPrincipal(HttpRequest request)
        {
            return GetPrincipal(request.Cookies);
        }
        private IPrincipal GetPrincipal(HttpCookieCollection cookies)
        {
            var session = GetSessions(cookies).ToArray();
            return !session.Any() ? SessionToPrincipal(session) : null;
        }
        public IEnumerable<ISession> GetSessions(HttpCookieCollection cookies)
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

        public void SetPrincipal(Controller controller, ISession session)
        {
            var sessions = GetSessions(controller.Request.Cookies).ToDictionary(x => x.Provider);
            sessions.Add(session.Provider, session);
            var jObject = JObject.FromObject(sessions);
            var json = jObject.ToString(Formatting.None);
            var authTicket = new FormsAuthenticationTicket(1, "userNameGoesHere", DateTime.UtcNow, DateTime.MaxValue, true, json, "CallWallAuth");
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            controller.Response.Cookies.Add(faCookie);
        }

        public void LogOff()
        {
            FormsAuthentication.SignOut();
        }
        private static IPrincipal SessionToPrincipal(IEnumerable<ISession> sessions)
        {
            var sessionGroups = sessions.Select(GetClaims);
            return new ClaimsPrincipal(sessionGroups.Select(c => new ClaimsIdentity(c, AuthenticationTypes.Password)));
        }

        private static List<Claim> GetClaims(ISession session)
        {
            var claims = session.AuthorizedResources
                                .Select(resource => new Claim(ResourceTypeKey, resource))
                                .ToList();
            claims.AddRange(new[]
                {
                    new Claim(ProviderTypeKey, session.Provider),
                    new Claim(AccessTokenTypeKey, session.AccessToken),
                    new Claim(RefreshTokenTypeKey, session.RefreshToken),
                    new Claim(ExpiryTypeKey, session.Expires.ToString("o"))
                });
            return claims;
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