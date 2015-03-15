using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Accounts;
using CallWall.Web.EventStore.Tests.Doubles;
using CallWall.Web.EventStore.Users;
using NSubstitute;
using NUnit.Framework;
using TestStack.BDDfy;

namespace CallWall.Web.EventStore.Tests
{
    [TestFixture]
    [Story(Title = "Signed in User adds an account",
        AsA = "As a user",
        IWant = "I want to add additional account",
        SoThat = "So that I can see aggregate contact information for multiple sources.")]
    [Timeout(20000)]
    public class UserAddsAccountFixture
    {
        #region Setup/TearDown

        private InMemoryEventStoreConnectionFactory _connectionFactory;
        private IEventStoreClient _eventStoreClient;

        [SetUp]
        public void SetUp()
        {
            _connectionFactory = new InMemoryEventStoreConnectionFactory();
            _eventStoreClient = new EventStoreClient(_connectionFactory, new ConsoleLoggerFactory());
        }

        [TearDown]
        public void TearDown()
        {
            _connectionFactory.Dispose();
        }

        #endregion

        [Test]
        public void UserAddsAccount()
        {
            new UserWithSingleAccountAddsAccount(_eventStoreClient)
               .Given(s => s.Given_a_signed_in_user())
               .When(s => s.When_user_adds_account())
               .Then(s => s.Then_user_has_all_accounts())
               .TearDownWith(s => s.Dispose())
               .BDDfy();
        }

        public class UserWithSingleAccountAddsAccount : IDisposable
        {
            private readonly UserRepository _userRepository;            
            private readonly IAccount _initialAccount;
            private readonly IAccount _newAccount;
            private readonly IList<IAccount> _allAccounts = new List<IAccount>();
            private User _initialUser;
            private User _updatedUser;

            public UserWithSingleAccountAddsAccount(IEventStoreClient eventStoreClient)
            {
                var accountFactory = new AccountFactory();
                _userRepository = new UserRepository(eventStoreClient, new ConsoleLoggerFactory(), accountFactory, Substitute.For<IAccountContactRefresher>());

                _initialAccount = new StubAccount();
                _allAccounts.Add(_initialAccount);

                _newAccount = new StubAccount
                {
                    AccountId = "OtherAccount",
                    DisplayName = "Other Account"
                };
                _allAccounts.Add(_newAccount);
            }

            public UserRepository UserRepository { get { return _userRepository; } }

            public async Task Given_a_signed_in_user()
            {
                await UserRepository.Run();
                await UserRepository.Login(_initialAccount);
                _initialUser = await _userRepository.Login(_initialAccount);
            }

            public async Task When_user_adds_account()
            {
                _updatedUser = await UserRepository.RegisterAccount(_initialUser.Id, _newAccount);
            }

            public void Then_user_has_all_accounts()
            {
                CollectionAssert.AreEqual(_allAccounts, _updatedUser.Accounts);
            }

            public void Dispose()
            {
                _userRepository.Dispose();
            }
        }
    }
}