using CallWall.Web.Logging;

namespace CallWall.Web
{
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