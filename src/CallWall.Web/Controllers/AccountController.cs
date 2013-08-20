using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CallWall.Web.Providers;
using CallWall.Web.Providers.Google;

namespace CallWall.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISecurityProvider _securityProvider;

        public AccountController(ISecurityProvider securityProvider)
        {
            _securityProvider = securityProvider;
        }

        public ActionResult Register()
        {
            return View();
        }

        //TODO: put the RegisterOAuth method here and rip the logic out of the home controller


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
            return new RedirectResult("/");
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult OAuthProviderList()
        {
            var accountProviders = _securityProvider.GetAccountConfigurations();
            return PartialView("_OAuthAccountListPartial", accountProviders);
        }

        [AllowAnonymous, AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Authenticate(string account, string[] resource)
        {
            var callBackUri = CreateCallBackUri();
            var authenticator = _securityProvider.GetAuthenticationProvider(account);
            var redirectUri = authenticator.AuthenticationUri(
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
            //TODO: _securityProvider should understand which provider to use based on the state. i.e. Google is the Account.
            var ap = _securityProvider.GetAuthenticationProvider("Google");

            var session = ap.CreateSession(code, state);

            _securityProvider.SetPrincipal(this, session);
            Response.Redirect("~/");
        }
    }
}
