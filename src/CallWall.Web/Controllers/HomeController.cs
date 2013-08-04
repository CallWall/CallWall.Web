using System;
using System.Web.Mvc;
using CallWall.Web.Providers;
using CallWall.Web.Providers.Google;

namespace CallWall.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Request.Cookies.Clear();
            Response.Cookies.Clear();
            var securityProvider = new SecurityProvider();
            if (securityProvider.IsAuthenticated(this))
            {
                return View();
            }
            return View("About");
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }


        public void GoogleAuth()
        {
            var gAuth = new GoogleAuthentication();
            var callBackUri = CreateCallBackUri();
            var redirectUri = gAuth.AuthenticationUri(
                callBackUri,
                new[] { "https://mail.google.com", "https://www.google.com/m8/feeds/" });

            Response.Redirect(redirectUri.ToString());
        }

        private string CreateCallBackUri()
        {
            var serverName = System.Web.HttpContext.Current.Request.Url;
            var callbackUri = new UriBuilder(serverName.Scheme, serverName.Host, serverName.Port, "home/oauth2callback");
            return callbackUri.ToString();
        }

        [AllowAnonymous]
        public ActionResult Oauth2Callback(string code, string state)
        {
            var auth = new GoogleAuthentication();
            var session = auth.CreateSession(code, state);

            var securityProvider = new SecurityProvider();
            securityProvider.AddSessionToUser(this, session, "google");

            return View("Index");
        }
    }

    //public class SessionSecurity
    //{
    //    public static void CreateSession()
    //    {
    //        string sess = CreateSessionKey();

    //        var principal = new ClaimsPrincipal(new[]
    //                {
    //                    new ClaimsIdentity(new[]
    //                        {
    //                            new Claim(ClaimTypes.Name, "myusername"), 
    //                            new Claim(ClaimTypes.Sid, sess),
    //                            //new Claim("RefreshToken", "Myrefresh token", ClaimValueTypes.String,"CallWall","Google", new ClaimsIdentity(...))
    //                        }, AuthenticationTypes.Password)
    //                });


    //        var token = FederatedAuthentication.SessionAuthenticationModule.CreateSessionSecurityToken(principal, "mycontext", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), false);

    //        FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(token);
    //    }

    //    private static string CreateSessionKey()
    //    {
    //        var rng = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
    //        //var rng = System.Security.Cryptography.RandomNumberGenerator.Create();

    //        var bytes = new byte[32];

    //        rng.GetNonZeroBytes(bytes);

    //        return Convert.ToBase64String(bytes);
    //    }
    //}
}
