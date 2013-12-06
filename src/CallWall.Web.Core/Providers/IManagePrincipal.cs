using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace CallWall.Web.Providers
{
    public interface IManagePrincipal
    {
        IPrincipal GetPrincipal(HttpRequest request);
        void SetPrincipal(Controller controller, ISession session);
        void LogOff();
    }
}