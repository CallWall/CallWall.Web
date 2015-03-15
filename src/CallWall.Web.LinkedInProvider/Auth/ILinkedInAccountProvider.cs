using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.LinkedInProvider.Auth
{
    public interface ILinkedInAccountProvider
    {
        Task<IAccount> CreateAccount(ISession session);
    }
}