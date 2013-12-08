﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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
            if (!FormsAuthentication.CookiesSupported) return null;
            HttpCookie authCookie = request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket == null)
                    return null;

                var session = GetSession(authTicket);
                if (session != null)
                {
                    return SessionToPrincipal(session);
                }
            }
            return null;
        }

        public void SetPrincipal(Controller controller, ISession session)
        {
            var state = session.Serialize();
            var authTicket = new FormsAuthenticationTicket(1, "userNameGoesHere", DateTime.UtcNow, DateTime.MaxValue, true, state, "CallWallAuth");
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            controller.Response.Cookies.Add(faCookie);
        }

        public void LogOff()
        {
            FormsAuthentication.SignOut();
        }
        private static IPrincipal SessionToPrincipal(ISession session)
        {
            var claims = session.AuthorizedResources
                                .Select(r => new Claim(ResourceTypeKey, r.ToString()))
                                .ToList();
            claims.AddRange(new[]
                {
                    new Claim(ProviderTypeKey, session.Provider),
                    new Claim(AccessTokenTypeKey, session.AccessToken),
                    new Claim(RefreshTokenTypeKey, session.RefreshToken),
                    new Claim(ExpiryTypeKey, session.Expires.ToString("o"))
                });

            var principal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(claims, AuthenticationTypes.Password)
                });
            return principal;
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