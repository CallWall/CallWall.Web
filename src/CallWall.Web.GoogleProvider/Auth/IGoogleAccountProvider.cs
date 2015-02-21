using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Auth
{
    public interface IGoogleAccountProvider
    {
        Task<IAccount> CreateAccount(ISession session);
    }
}