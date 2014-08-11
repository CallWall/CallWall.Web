using CallWall.Web.EventStore.Domain;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubAccount : Account
    {
        public StubAccount(IUserRepository userRepository, IAccountContacts accountContacts)
            : base(userRepository, accountContacts)
        {
            Provider = "TestProvider";
            AccountId = "Test.User@email.com";
            DisplayName = "Test User";
            CurrentSession = new StubSession();
        }
    }
}