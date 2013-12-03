using System;
using System.Web.Mvc;
using CallWall.Web.Providers;

namespace CallWall.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISecurityProvider _securityProvider;
        private readonly IAuthenticationProviderGateway _authenticationProviderGateway;

        public AccountController(ISecurityProvider securityProvider, IAuthenticationProviderGateway authenticationProviderGateway)
        {
            _securityProvider = securityProvider;
            _authenticationProviderGateway = authenticationProviderGateway;
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
            _securityProvider.LogOff();
            return new RedirectResult("/");
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult OAuthProviderList()
        {
            var accountProviders = _authenticationProviderGateway.GetAccountConfigurations();
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

        private string CreateCallBackUri()
        {
            var serverName = System.Web.HttpContext.Current.Request.Url;
            var callbackUri = new UriBuilder(serverName.Scheme, serverName.Host, serverName.Port, "Account/oauth2callback");
            return callbackUri.ToString();
        }

        [AllowAnonymous]
        public void Oauth2Callback(string code, string state)
        {
            var session = _securityProvider.CreateSession(code, state);

            _securityProvider.SetPrincipal(this, session);
            Response.Redirect("~/");
        }
    }
}
