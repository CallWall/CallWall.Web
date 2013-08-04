using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CallWall.Web.Providers.Google;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Providers
{
    public class SecurityProvider
    {
        private const string AuthCookieKey = "Auth";

        public bool IsAuthenticated(Controller controller)
        {
            //return User.Identity.IsAuthenticated;
            //return Session["GoogleSession"] != null;

            var authCookie = controller.Request.Cookies[AuthCookieKey];
            return authCookie != null 
                && !string.IsNullOrEmpty(authCookie["google"]); //HACK: This should not know about the google key.
        }

        public void AddSessionToUser(Controller controller, ISession session, string key)
        {
            var jsonSession = session.ToJson();
            var base64Data = jsonSession.ToBase64String();
            var authCookie = new HttpCookie(AuthCookieKey);
            authCookie.Expires = DateTime.MaxValue;
            authCookie.Values.Add(key, base64Data);
            
            controller.Response.Cookies.Add(authCookie);
        }

        public ISession GetSession(HubCallerContext context, string key)
        {
           
            var authCookie = context.RequestCookies["Auth"];
            if (authCookie == null || string.IsNullOrWhiteSpace(authCookie.Value))
                return null;
            var data = authCookie.Value;//Hmmm not way to index into the keys of a cookie....

            //HACK: Assumes only 1 value in cookie. Should move to Claims based model anyway.
            var base64 = data.Substring((key+"=").Length);
            var json = base64.FromBase64String();
            var session = json.FromJson();
            return session;
        }
    }

    public static class StringExtensions
    {
        public static string ToBase64String(this string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            var base64 = Convert.ToBase64String(bytes);
            return base64;
        }
        public static string FromBase64String(this string source)
        {
            var bytes = Convert.FromBase64String(source);
            var output = Encoding.UTF8.GetString(bytes);
            return output;
        }
    }
}