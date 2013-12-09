using CallWall.Web;
using $rootnamespace$.Logging;

namespace $rootnamespace$
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private readonly ILogger _logger;

        public MvcApplication()
        {
            _logger =new Log4NetLogger(GetType());
        }

        protected void Application_Start()
        {
            _logger.Info("Starting Application...");
            // The Startup class now does most of the work to play nicely with OWIN.
            _logger.Info("Application started.");
        }

        public override void Dispose()
        {
            _logger.Info("Application being disposed.");
            base.Dispose();
        }
    }
}