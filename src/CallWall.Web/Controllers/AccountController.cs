using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CallWall.Web.Domain;
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
            FormsAuthentication.SignOut();
            return new RedirectResult("/");
        }

        [AllowAnonymous]
        [ChildActionOnly]
        [AsyncTimeout(2000)]
        public async Task<ActionResult> CurrentRegisteredAccountList()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new EmptyResult();
            }
            var userId = User.UserId();
            var user =  await _loginProvider.GetUser(userId);

            var authorizedAccounts = from acc in user.Accounts
                    from provider in _authenticationProviderGateway.GetProviderConfigurations()
                    where acc.Provider == provider.Name
                    select new OAuthAccountListItem(acc, provider);

            return PartialView("_OAuthAccountListPartial", authorizedAccounts);
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult OAuthProviderList()
        {
            var providers = _authenticationProviderGateway.GetProviderConfigurations()
                                                                 .Select(ap => new OAuthProviderListItem(ap));

            return PartialView("_OAuthProviderListPartial", providers);
        }

        [AllowAnonymous, AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Authenticate(string providerName, string[] resource)
        {
            var callBackUri = CreateCallBackUri();

            var redirectUri = _authenticationProviderGateway.AuthenticationUri(providerName,
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
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.UserId();
                var version = User.TicketVersion();
                var user = await _loginProvider.RegisterAccount(userId, code, state);
                SetPrincipal(user, version+1);    
            }
            else
            {
                var user = await _loginProvider.Login(code, state);
                SetPrincipal(user);    
            }
            
            return new RedirectResult("~/");
        }

        private void SetPrincipal(User user, int version = 1)
        {
            var authTicket = new FormsAuthenticationTicket(version, user.DisplayName, 
                DateTime.UtcNow, DateTime.MaxValue, true, user.Id.ToString(), "CallWallAuth");
            
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            Response.Cookies.Add(faCookie);
        }
    }
}
