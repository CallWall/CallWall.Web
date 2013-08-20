using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using CallWall.Web.Providers;

namespace CallWall.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Bootstrapper.Initialise();
        }

        protected void FormsAuthentication_OnAuthentication(object sender, FormsAuthenticationEventArgs args)
        {
            //HACK: How do I inject the security provider into the global asax?
            //Perhaps this : http://www.hanselman.com/blog/IPrincipalUserModelBinderInASPNETMVCForEasierTesting.aspx 
            //var securityProvider = new SecurityProvider();
            var principal = SecurityProvider.GetPrincipal(args.Context.Request);
            if (principal != null)
                args.Context.User = principal;
        }
    }
}