using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Tests.Doubles;
using NUnit.Framework;
using TestStack.BDDfy;

namespace CallWall.Web.EventStore.Tests
{
    [TestFixture]
    [Story(Title = "New User registers",
        AsA = "As an anon user",
        IWant = "I want to register with CallWall",
        SoThat = "So that I can use the system")]
    public class NewUserRegistration
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
        public void UserRegistersSuccussfully()
        {
            new NewUserScenario(_connectionFactory)
                .Given(s => s.Given_an_anon_user())
                .When(s => s.When_the_user_registers_with_an_unrecongised_account())
                .Then(s => s.Then_a_user_is_created())
                .Then(s => s.Then_an_account_is_created_with_an_account_identifier())
                .Then(s => s.Then_an_account_is_created_with_the_provider())
                .Then(s => s.Then_an_account_is_created_with_permissions_mapped_to_provider_scopes())
                .BDDfy();
        }
        //What if the account already exists? What is the least awful thing to do? I think it is to just let them log in. What if they choose different permissions??


        [Test]
        public void UserLogin()
        {
            new UserWithSingleAccountLogsInScenario(_connectionFactory)
               .Given(s => s.Given_an_existing_user())
               .When(s => s.When_user_logs_in_by_account())
               .Then(s => s.Then_user_has_all_accounts())
               .BDDfy();
        }

        public class NewUserScenario
        {
            private User _user;
            private IAccount _account;
            private readonly UserRepository _userRepository;
            private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(2);
            public NewUserScenario(IEventStoreConnectionFactory connectionFactory)
            {
                _userRepository = new UserRepository(connectionFactory);
                _userRepository.Load()
                    .Wait(TimeOut);
            }

            public void Given_an_anon_user()
            {
                _user = User.AnonUser;
            }

            public async Task When_the_user_registers_with_an_unrecongised_account()
            {
                _account = new StubAccount(_userRepository);
                _account.CurrentSession.AuthorizedResources.Add("email");
                _account.CurrentSession.AuthorizedResources.Add("calendar");
                _user = await _userRepository.RegisterNewUser(_account, Guid.NewGuid());
            }

            public void Then_a_user_is_created()
            {
                Assert.IsNotNull(_user);
                Assert.AreNotSame(User.AnonUser, _user);
            }

            public void Then_an_account_is_created_with_an_account_identifier()
            {
                var actualAccount = _user.Accounts.Single();
                Assert.AreEqual(_account.AccountId, actualAccount.AccountId);
            }

            public void Then_an_account_is_created_with_the_provider() //(e.g. gmail/twitter/etc....)
            {
                var actualAccount = _user.Accounts.Single();
                Assert.AreEqual(_account.Provider, actualAccount.Provider);
            }

            public void Then_an_account_is_created_with_permissions_mapped_to_provider_scopes()
            //With permission key set and the mapping to the provider's OAuth Scopes e.g. {Key: "CallWall.Communications", Value: "https://mail.google.com/"}
            {
                var actualAccount = _user.Accounts.Single();
                CollectionAssert.AreEqual(_account.CurrentSession.AuthorizedResources, actualAccount.CurrentSession.AuthorizedResources);
            }

            public void Then_an_AccountContactRefreshCommand_is_issued_for_the_account()
            {
                Assert.Inconclusive();
            }
        }

        public class UserWithSingleAccountLogsInScenario
        {
            private readonly UserRepository _userRepository;
            private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(2);
            private User _storedUser = User.AnonUser;
            private IAccount _account;
            private readonly IList<IAccount> _allAccounts = new List<IAccount>();

            public UserWithSingleAccountLogsInScenario(IEventStoreConnectionFactory connectionFactory)
            {
                _userRepository = new UserRepository(connectionFactory);
                UserRepository.Load()
                    .Wait(TimeOut);
            }

            public UserRepository UserRepository { get { return _userRepository; } }

            public async Task Given_an_existing_user()
            {
                _account = new StubAccount(_userRepository);
                _allAccounts.Add(_account);
                await UserRepository.RegisterNewUser(_account, Guid.NewGuid());
            }

            public async Task When_user_logs_in_by_account()
            {
                _storedUser = await _account.Login();
            }

            public void Then_user_has_all_accounts()
            {
                CollectionAssert.AreEqual(_allAccounts, _storedUser.Accounts);
            }

            public void Then_an_AccountContactRefresh_command_is_issued_for_the_account()
            {
                throw new NotImplementedException();
            }

        }
    }
}