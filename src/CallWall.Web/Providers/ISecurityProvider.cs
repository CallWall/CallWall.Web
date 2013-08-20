using System.Collections.Generic;
using System.Web.Mvc;

namespace CallWall.Web.Providers
{
    public interface ISecurityProvider
    {
        //IPrincipal GetPrincipal(HttpRequest request);
        void SetPrincipal(Controller controller, ISession session);
        void LogOff();

        IAccountAuthentication GetAuthenticationProvider(string account);
        IEnumerable<IAccountConfiguration> GetAccountConfigurations();
    }
}