using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CallWall.Web.Providers.Google;

namespace CallWall.Web.Providers
{
    public class SecurityProvider : ISecurityProvider
    {
        public void SetPrincipal(Controller controller,ISession session)
        {
            var authTicket = new FormsAuthenticationTicket(1, "userNameGoesHere", DateTime.UtcNow, DateTime.MaxValue, true, session.ToJson(), "CallWallAuth");
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            controller.Response.Cookies.Add(faCookie);
        }

        public void LogOff()
        {
            FormsAuthentication.SignOut();
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

                var session = authTicket.UserData.FromJson();

                return SessionToPrincipal(session);
            }
            return null;
        }

        private static IPrincipal SessionToPrincipal(ISession session)
        {
            var claims = session.AuthorizedResources
                                .Select(r => new Claim("http://callwall.com/identity/Resource", r.ToString()))
                                .ToList();
            claims.AddRange(new[]
                {
                    new Claim("http://callwall.com/identity/AccessToken", session.Provider),
                    new Claim("http://callwall.com/identity/AccessToken", session.AccessToken),
                    new Claim("http://callwall.com/identity/AccessToken", session.RefreshToken),
                    new Claim("http://callwall.com/identity/AccessToken", session.Expires.ToString("o"))
                });

            var principal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(claims, AuthenticationTypes.Password)
                });
            return principal;
        }
    }
}