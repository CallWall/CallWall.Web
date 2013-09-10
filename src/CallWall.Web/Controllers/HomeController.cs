using System;
using System.Web.Mvc;
using CallWall.Web.Providers;


namespace CallWall.Web.Controllers
{
    //TODO Move google auth to a google controller!
    public class HomeController : Controller
    {
        //private readonly IGoogleAuthentication _googleAuthentication;
        private readonly ISecurityProvider _securityProvider;

        //public HomeController(IGoogleAuthentication googleAuthentication, ISecurityProvider securityProvider)
        //{
        //    _googleAuthentication = googleAuthentication;
        //    _securityProvider = securityProvider;
        //}

        public HomeController(ISecurityProvider securityProvider)
        {
            _securityProvider = securityProvider;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            //TODO: I think this can be done with Attributes? Is that what I want? Is that easy to test?
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            return View("About");
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            Console.WriteLine(User.Identity);
            return View();
        }

        [AllowAnonymous]
        public ActionResult Download()
        {
            return View();
        }


        //public void GoogleAuth()
        //{
        //    var callBackUri = CreateCallBackUri();
        //    var redirectUri = _googleAuthentication.AuthenticationUri(
        //        callBackUri,
        //        new[] { "https://mail.google.com", "https://www.google.com/m8/feeds/" });

        //    Response.Redirect(redirectUri.ToString());
        //}

        //private string CreateCallBackUri()
        //{
        //    var serverName = System.Web.HttpContext.Current.Request.Url;
        //    var callbackUri = new UriBuilder(serverName.Scheme, serverName.Host, serverName.Port, "home/oauth2callback");
        //    return callbackUri.ToString();
        //}

        //[AllowAnonymous]
        //public void Oauth2Callback(string code, string state)
        //{
        //    var session = _googleAuthentication.CreateSession(code, state);

        //    _securityProvider.SetPrincipal(this, session);
        //    Response.Redirect("~/");
        //}
    }
}