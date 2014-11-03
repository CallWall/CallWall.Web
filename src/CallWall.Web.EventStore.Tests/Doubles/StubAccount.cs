using CallWall.Web.Domain;
using CallWall.Web.EventStore.Accounts;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubAccount : Account
    {
        public StubAccount()
        {
            Provider = "TestProvider";
            AccountId = "Test.User@email.com";
            DisplayName = "Test User";
            Handles = new ContactHandle[] {new ContactEmailAddress("Test.User@email.com", "Main")};
            CurrentSession = new StubSession();
        }
    }
}