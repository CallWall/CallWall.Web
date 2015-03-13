using System;
using System.Threading.Tasks;

namespace CallWall.Web.Domain
{
    public interface IUserRepository : IRunnable, IDisposable
    {
        Task<User> Login(IAccount account);

        Task<User> RegisterAccount(Guid userId, IAccount account);

        Task<User> GetUserBy(Guid userId);
    }
}