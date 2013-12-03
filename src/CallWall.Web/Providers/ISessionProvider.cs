using System.Security.Principal;

namespace CallWall.Web.Providers
{
    public interface ISessionProvider
    {
        ISession CreateSession(string code, string state);
        ISession GetSession(IPrincipal user);
    }
}