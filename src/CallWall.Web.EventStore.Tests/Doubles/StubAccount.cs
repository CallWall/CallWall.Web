using CallWall.Web.EventStore.Accounts;
using CallWall.Web.EventStore.Users;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubAccount : Account
    {
        public StubAccount(IUserRepository userRepository, IAccountContactRefresher accountContactRefresher)
            : base(userRepository, accountContactRefresher)
        {
            Provider = "TestProvider";
            AccountId = "Test.User@email.com";
            DisplayName = "Test User";
            CurrentSession = new StubSession();
        }
    }
}