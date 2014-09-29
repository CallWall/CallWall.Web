using System;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using CallWall.Web.Models;
using CallWall.Web.Providers;

namespace CallWall.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationProviderGateway _authenticationProviderGateway;
        private readonly ILoginProvider _loginProvider;

        public AccountController(IAuthenticationProviderGateway authenticationProviderGateway,
                                 ILoginProvider loginProvider)
        {
            _authenticationProviderGateway = authenticationProviderGateway;
            _loginProvider = loginProvider;
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult LogIn()
        {
            return View();
        }

        public ActionResult Manage()
        {
            return View();
        }

        public ActionResult LogOff()
        {
            //_sessionProvider.LogOff();
            FormsAuthentication.SignOut();
            return new RedirectResult("/");
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult OAuthProviderList()
        {
            //TODO : Need to be able to get a list of Providers that a User already has registered with. -LC
            //var activeProviders = _sessionProvider.GetSessions(User).Select(s => s.Provider);
            //var accountProviders = _authenticationProviderGateway.GetAccountConfigurations()
            //                                                     .Select(ap => new OAuthAccountListItem(ap, activeProviders.Contains(ap.Name)));

            var accountProviders = _authenticationProviderGateway.GetAccountConfigurations()
                                                                 .Select(ap => new OAuthAccountListItem(ap, false));

            //var accountProviders = Enumerable.Empty<OAuthAccountListItem>();
            return PartialView("_OAuthAccountListPartial", accountProviders);
        }

        [AllowAnonymous, AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Authenticate(string account, string[] resource)
        {
            var callBackUri = CreateCallBackUri();

            var redirectUri = _authenticationProviderGateway.AuthenticationUri(account,
                callBackUri,
                resource);

            return new RedirectResult(redirectUri.ToString());
        }

        private static string CreateCallBackUri()
        {
            var serverName = System.Web.HttpContext.Current.Request.Url;
            var callbackUri = new UriBuilder(serverName.Scheme, serverName.Host, serverName.Port, "Account/oauth2callbackAsync");
            return callbackUri.ToString();
        }

        [AllowAnonymous]
        [AsyncTimeout(2000)]
        public async Task<ActionResult> Oauth2CallbackAsync(string code, string state)
        {
            var user =  await _loginProvider.Login(code, state);
            SetPrincipal(user);
            return new RedirectResult("~/");
        }

        private void SetPrincipal(User user)
        {
            var authTicket = new FormsAuthenticationTicket(1, user.DisplayName, 
                DateTime.UtcNow, DateTime.MaxValue, true, user.Id.ToString(), "CallWallAuth");
            
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            Response.Cookies.Add(faCookie);
        }
    }
}
