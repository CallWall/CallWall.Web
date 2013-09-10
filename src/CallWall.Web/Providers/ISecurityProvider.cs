using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace CallWall.Web.Providers
{
    public interface ISecurityProvider
    {
        IPrincipal GetPrincipal(HttpRequest request);
        ISession GetSession(IPrincipal user);
        void SetPrincipal(Controller controller, ISession session);
        void LogOff();

        IAccountAuthentication GetAuthenticationProvider(string account);
        IEnumerable<IAccountConfiguration> GetAccountConfigurations();
    }
}