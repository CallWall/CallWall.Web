using System;
using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface ILoginProvider
    {
        Task<User> Login(string oAuthCode, string oAuthState);
        Task<User> GetUser(Guid userId);
    }
}