using System;
using System.Threading.Tasks;

namespace CallWall.Web.EventStore.Users
{
    public interface IUserRepository : IDomainEventBase
    {
        Task<User> FindByAccount(IAccount account);
        Task<User> RegisterNewUser(IAccount account, Guid eventId);
    }
}