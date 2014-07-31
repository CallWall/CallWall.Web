using System;
using System.Linq;
using System.Web.Mvc;
using CallWall.Web.Models;
using CallWall.Web.Providers;

namespace CallWall.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationProviderGateway _authenticationProviderGateway;
        private readonly ISessionProvider _sessionProvider;

        public AccountController(IAuthenticationProviderGateway authenticationProviderGateway,
                                 ISessionProvider sessionProvider)
        {
            _authenticationProviderGateway = authenticationProviderGateway;
            _sessionProvider = sessionProvider;
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
            _sessionProvider.LogOff();
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
            var accountProviders = Enumerable.Empty<OAuthAccountListItem>();
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
            var callbackUri = new UriBuilder(serverName.Scheme, serverName.Host, serverName.Port, "Account/oauth2callback");
            return callbackUri.ToString();
        }

        [AllowAnonymous]
        public void Oauth2Callback(string code, string state)
        {
            var session = _sessionProvider.CreateSession(code, state);

            _sessionProvider.SetPrincipal(this, session);
            Response.Redirect("~/");
        }
    }
}
