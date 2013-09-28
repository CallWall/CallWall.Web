using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using CallWall.Web.Logging;
using CallWall.Web.Providers;
using Microsoft.Practices.Unity;

namespace CallWall.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private readonly ILogger _logger;
        private IUnityContainer _container;

        public MvcApplication()
        {
            _logger =new Log4NetLogger(GetType());
        }

        protected void Application_Start()
        {
            _logger.Info("Starting Application...");
            _container = Bootstrapper.Initialise();
            RegisterHubs.Start(_container);
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            _logger.Info("Application started.");
        }

        protected void FormsAuthentication_OnAuthentication(object sender, FormsAuthenticationEventArgs args)
        {
            _logger.Info("Authentication user...");
            //HACK: How do I inject the security provider into the global asax?
            //Perhaps this : http://www.hanselman.com/blog/IPrincipalUserModelBinderInASPNETMVCForEasierTesting.aspx 
            var securityProvider = _container.Resolve<ISecurityProvider>();

            var principal = securityProvider.GetPrincipal(args.Context.Request);
            if (principal != null)
            {
                args.Context.User = principal;
                _logger.Info("User authenticated.");
            }
            else
            {
                _logger.Info("User is anonymous.");
            }
        }

        public override void Dispose()
        {
            _logger.Info("Application being disposed.");
            using (_container)
            {}

            base.Dispose();
        }
    }
}