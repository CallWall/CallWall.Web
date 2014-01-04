using System;
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
        
        void Application_Error(object sender, EventArgs e)
        {
            //see http://msdn.microsoft.com/en-us/library/24395wz3(v=vs.100).aspx for more details
            if (_logger != null)
                _logger.Error(Server.GetLastError(), "Application error");
        }
    }
}