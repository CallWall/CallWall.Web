using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using CallWall.Web.Providers.Google;

namespace CallWall.Web.Providers
{
    public interface ISecurityProvider
    {
        IPrincipal GetPrincipal(HttpRequest request);
        void SetPrincipal(Controller controller, ISession session);
        void LogOff();
    }
}