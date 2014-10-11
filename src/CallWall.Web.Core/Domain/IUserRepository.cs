using System;
using System.Threading.Tasks;

namespace CallWall.Web.Domain
{
    public interface IUserRepository : IRunnable, IDisposable
    {
        Task<User> Login(IAccount account);
        //TODO: Remove the RegisterNewUser. Just have login. internally we will identify if it is a return visitor or a new registration. -LC
        Task<User> RegisterNewUser(IAccount account, Guid eventId);

        Task<User> GetUserBy(Guid userId);
    }
}