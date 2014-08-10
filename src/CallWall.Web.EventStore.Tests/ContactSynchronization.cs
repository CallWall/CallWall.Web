using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Tests.Doubles;
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
            private IAccount _account;
            private readonly UserRepository _userRepository;
            private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(2);
            public UserLoginScenario(IEventStoreConnectionFactory connectionFactory)
            {
                _userRepository = new UserRepository(connectionFactory);
                _userRepository.Load()
                    .Wait(TimeOut);
            }

            public void GivenAnAnonUser()
            {
                _user = User.AnonUser;
            }

            public async Task WhenTheUserRegistersWithAnUnrecongisedAccount()
            {
                _account = new StubAccount(_userRepository);
                _account.CurrentSession.AuthorizedResources.Add("email");
                _account.CurrentSession.AuthorizedResources.Add("calendar");
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
                Assert.Inconclusive();
            }
        }

        public class UserWithSingleAccountLogsInScenario
        {
            private readonly UserRepository _userRepository;
            private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(2);
            private IAccount _account;
            private User _storedUser;

            public UserWithSingleAccountLogsInScenario(IEventStoreConnectionFactory connectionFactory)
            {
                _userRepository = new UserRepository(connectionFactory);
                _userRepository.Load()
                    .Wait(TimeOut);
            }

            public async Task Given_an_existing_user()
            {
                _account = new StubAccount(_userRepository);
                await _userRepository.RegisterNewUser(_account, Guid.NewGuid());
            }

            public async Task When_User_logs_in()
            {
                _storedUser = await _account.Login();
            }

            public void Then_an_AccountContactRefresh_command_is_issued_for_the_account()
            {
                throw new NotImplementedException();
            }


        }
    }
}