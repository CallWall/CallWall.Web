using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography;
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
        //TODO: When the EventStore Chocolatey package is corrected, then run from ~\Server\EventStore\EventStore.SingleNode.exe
        private const string EventStorePath = @"C:\Program Files\eventstore\EventStore-NET-v3.0.0rc2\EventStore.SingleNode.exe";
        private InMemoryEventStoreConnectionFactory _connectionFactory;
        [SetUp]
        public void SetUp()
        {
            _connectionFactory = new InMemoryEventStoreConnectionFactory(EventStorePath, "127.0.0.1", 1113);
        }

        [TearDown]
        public void TearDown()
        {
            _connectionFactory.Dispose();
        }

        [Test]
        public void UserRegistersSuccussfully()
        {
            new NewUserScenario(_connectionFactory)
                .Given(s => s.GivenAnAnonUser())
                .When(s => s.WhenTheUserRegistersWithAnUnrecongisedAccount())
                .Then(s => s.ThenAUserIsCreated())
                .Then(s => s.ThenAnAccountIsCreatedWithAnAccountIdentifier())
                .Then(s => s.ThenAnAccountIsCreatedWithTheProvider())
                .Then(s => s.ThenAnAccountIsCreatedWithPermissionsMappedToProviderScopes())
                //        .Then(s => s.ThenTheNewAccountIsAssociatedToTheNewUser())
                //.Then(s => s.ThenAnAccountContactRefreshCommandIsIssuedForTheAccount())
                .BDDfy();
        }
        //What if the account already exists? What is the least awful thing to do? I think it is to just let them log in. What if they choose different permissions??


        [Test]
        public void UserLogin()
        {
            var account = new StubAccount();
            new SingleAccountUserLogin(_connectionFactory)
                .Given(s => s.GivenAnExistingUser(account))
                .When(s => s.WhenUserRetirevedByAccount(account))
                .Then(s => s.ThenUserHasAllAccounts())
                .BDDfy();
        }

        public class NewUserScenario
        {
            private CallWall.Web.User _user;
            private IAccount _account;
            private readonly UserRepository _userRepository;
            private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(2);
            public NewUserScenario(IEventStoreConnectionFactory connectionFactory)
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
                _account = new StubAccount();
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

        public class SingleAccountUserLogin
        {
            private readonly UserRepository _userRepository;
            private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(2);
            private User _inputUser = User.AnonUser;
            private User _storedUser = User.AnonUser;
            private List<IAccount> _allAccounts = new List<IAccount>();

            public SingleAccountUserLogin(IEventStoreConnectionFactory connectionFactory)
            {
                _userRepository = new UserRepository(connectionFactory);
                _userRepository.Load()
                    .Wait(TimeOut);
            }

            public async Task GivenAnExistingUser(IAccount account)
            {
                _allAccounts = new List<IAccount> { account };
                _inputUser = await _userRepository.RegisterNewUser(account, Guid.NewGuid());
            }

            public async Task WhenUserRetirevedByAccount(IAccount account)
            {
                _storedUser = await _userRepository
                    .IsUpToDate.Where(isUpToDate => isUpToDate)
                    .Select(isUpToDate => _userRepository.FindByAccount(account))
                    .Take(1)
                    .ToTask();
            }

            public void ThenUserHasAllAccounts()
            {
                CollectionAssert.AreEqual(_allAccounts, _storedUser.Accounts);
            }


        }

        /*GIVEN an anonymous user
	        WHEN they register (login without the account being registered)
		        THEN a User is created
		        THEN an Account is created
			        With an Account identifier
			        With the Provider (gmail/twitter/etc....)
			        With permission key set and the mapping to the provider's OAuth Scopes e.g. {Key: "CallWall.Communications", Value: "https://mail.google.com/"}
			        (THEN the saved Account is decorated with the available scopes/permissions the User agreed to.)
		        THEN the new Account is associated to the new User
		        THEN an AccountContact refresh command is issued for the account
		
	        WHEN they login (Login with an account that is registered)
		        THEN they are logged in as the user the account is associated with	 (implies some sort of look up. Is this in memory? Why not, it should just be a massive hashtable/dictionary.)
	*/
    }
}