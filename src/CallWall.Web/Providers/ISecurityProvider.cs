using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace CallWall.Web.Providers
{
    //TODO: Validate that all methods are still used. -LC
    public interface ISecurityProvider : ISessionProvider, IManagePrincipal
    {
      
    }

    public interface IManagePrincipal
    {
        IPrincipal GetPrincipal(HttpRequest request);
        void SetPrincipal(Controller controller, ISession session);
        void LogOff();
    }

    public interface ISessionProvider
    {
        ISession CreateSession(string code, string state);
        ISession GetSession(IPrincipal user);
    }
}