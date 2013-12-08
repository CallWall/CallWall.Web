using System.Collections.Generic;
using System.Security.Principal;
using System.Web.Mvc;

namespace CallWall.Web.Providers
{
    public interface ISessionProvider
    {
        ISession CreateSession(string code, string state);
        IEnumerable<ISession> GetSessions(IPrincipal user);
        ISession GetSession(IPrincipal user);
        void SetPrincipal(Controller controller, ISession session);
        void LogOff();
    }
}
