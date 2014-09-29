using CallWall.Web.EventStore.Accounts;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubAccount : Account
    {
        public StubAccount(IAccountContactRefresher accountContactRefresher)
        {
            Provider = "TestProvider";
            AccountId = "Test.User@email.com";
            DisplayName = "Test User";
            CurrentSession = new StubSession();
        }
    }
}