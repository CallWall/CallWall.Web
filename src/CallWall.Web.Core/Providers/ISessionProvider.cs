using System.Collections.Generic;
using System.Security.Principal;

namespace CallWall.Web.Providers
{
    public interface ISessionProvider
    {
        ISession CreateSession(string code, string state);
        IEnumerable<ISession> GetSessions(IPrincipal user);
    }
}