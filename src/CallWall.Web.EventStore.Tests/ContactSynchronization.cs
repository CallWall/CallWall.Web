using System;
using System.Linq;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Domain;
using CallWall.Web.EventStore.Tests.Doubles;
using NSubstitute;
using NUnit.Framework;
using TestStack.BDDfy;

namespace CallWall.Web.EventStore.Tests
{
    [TestFixture]
    [Story(Title = "User requests Contact updates",
        AsA = "As a registered user",
        IWant = "I want to synchronize my contacts from external systems",
        SoThat = "So that I can see my contact summaries as quickly as possible")]
    public class ContactSynchronization
    {
        #region Setup/TearDown

        private InMemoryEventStoreConnectionFactory _connectionFactory;

        [SetUp]
        public void SetUp()
        {
            _connectionFactory = new InMemoryEventStoreConnectionFactory();
        }

        [TearDown]
        public void TearDown()
        {
            _connectionFactory.Dispose();
        }

        #endregion


        [Test]
        public void UserLogin()
        {
            new UserWithSingleAccountLogsInScenario(_connectionFactory)
                .Given(s => s.Given_an_existing_user())
                .When(s => s.When_User_logs_in())
                .Then(s => s.Then_an_AccountContactRefresh_command_is_issued_for_the_account())
                .BDDfy();
        }


        public class UserLoginScenario
        {
            private User _user;
            private readonly IAccount _account;
            private readonly UserRepository _userRepository;
            private readonly IAccountContactRefresher _accountContactRefresherMock;

            public UserLoginScenario(IEventStoreConnectionFactory connectionFactory)
            {
                _accountContactRefresherMock = Substitute.For<IAccountContactRefresher>();
                var accountContactsFactory = Substitute.For<IAccountContactsFactory>();
                accountContactsFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_accountContactRefresherMock);

                var x = accountContactsFactory.Create("", null);

                _userRepository = new UserRepository(connectionFactory, accountContactsFactory);
                _userRepository.Run();
                
                _account = new StubAccount(_userRepository, _accountContactRefresherMock);
                _account.CurrentSession.AuthorizedResources.Add("email");
                _account.CurrentSession.AuthorizedResources.Add("calendar");
            }

            public void GivenAnAnonUser()
            {
                _user = User.AnonUser;
            }

            public async Task WhenTheUserRegistersWithAnUnrecongisedAccount()
            {
                
                _user = await _userRepository.RegisterNewUser(_account, Guid.NewGuid());
            }

            public void ThenAUserIsCreated()
            {
                Assert.IsNotNull(_user);
                Assert.AreNotSame(User.AnonUser, _user);
            }

            public void ThenAnAccountIsCreatedWithAnAccountIdentifier()
            {
                var actualAccount = _user.Accounts.Single();
                Assert.AreEqual(_account.AccountId, actualAccount.AccountId);
            }

            public void ThenAnAccountIsCreatedWithTheProvider() //(e.g. gmail/twitter/etc....)
            {
                var actualAccount = _user.Accounts.Single();
                Assert.AreEqual(_account.Provider, actualAccount.Provider);
            }

            public void ThenAnAccountIsCreatedWithPermissionsMappedToProviderScopes()
                //With permission key set and the mapping to the provider's OAuth Scopes e.g. {Key: "CallWall.Communications", Value: "https://mail.google.com/"}
            {
                var actualAccount = _user.Accounts.Single();
                CollectionAssert.AreEqual(_account.CurrentSession.AuthorizedResources, actualAccount.CurrentSession.AuthorizedResources);
            }

            public void ThenAnAccountContactRefreshCommandIsIssuedForTheAccount()
            {
                _accountContactRefresherMock.Received().RequestRefresh(ContactRefreshTriggers.Registered);
            }
        }

        public class UserWithSingleAccountLogsInScenario
        {
            private readonly UserRepository _userRepository;
            private readonly IAccount _account;
            private User _storedUser;
            private readonly IAccountContactRefresher _accountContactRefresherMock = Substitute.For<IAccountContactRefresher>();

            public UserWithSingleAccountLogsInScenario(IEventStoreConnectionFactory connectionFactory)
            {
                var accountContactsFactory = Substitute.For<IAccountContactsFactory>();
                accountContactsFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_accountContactRefresherMock);
                _userRepository = new UserRepository(connectionFactory, accountContactsFactory);
                _userRepository.Run();
                
                _account = new StubAccount(_userRepository, _accountContactRefresherMock);
            }

            public async Task Given_an_existing_user()
            {
                
                await _userRepository.RegisterNewUser(_account, Guid.NewGuid());
            }

            public async Task When_User_logs_in()
            {
                _storedUser = await _account.Login();
            }

            public void Then_an_AccountContactRefresh_command_is_issued_for_the_account()
            {
                _accountContactRefresherMock.Received().RequestRefresh(ContactRefreshTriggers.Login);
            }
        }
    }
}