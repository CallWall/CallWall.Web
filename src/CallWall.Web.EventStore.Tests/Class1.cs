using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Test]
        public void UserRegistersSuccussfully()
        {
            new NewUserScenario()
                .Given(s => s.GivenAnAnonUser())
                .When(s => s.WhenTheUserRegistersWithAnUnrecongisedAccount())
                .Then(s => s.ThenAUserIsCreated())
                .Then(s => s.ThenAnAccountIsCreatedWithAnAccountIdentifier())
                .Then(s => s.ThenAnAccountIsCreatedWithTheProvider())
                .Then(s => s.ThenAnAccountIsCreatedWithPermissionsMappedToProviderScopes())
                //        .Then(s => s.ThenTheNewAccountIsAssociatedToTheNewUser())
                .Then(s => s.ThenAnAccountContactRefreshCommandIsIssuedForTheAccount())
                .BDDfy();
        }
    }

    //CallWall has Users
    //Users have registered Accounts
    //Accounts have a PK of an internally generated integer
    //         have a unique constraint on the composite key of Provider+UserName
    //Accounts have sessions
    //Sessions have authentication and Authorization information
    //         can expire
    //         can be renewed
    //         can be revoked (which will revoke the Account)
    //Accounts have Contacts
    //Contacts have a PK of Provider+AccountId+ProviderId
    //Users have aggregated view of Contacts

    //EventStore        - Just a facade over the Actual Event store (if we need it)
    //UserRepository    - Gets Users. Will replay all over Users event stream into memory to be an internal cache for look ups (Read/Query only?)
    //                  - Find User by Account (e.g. for login)
    //UserFactory       - Creates Users from an Account (doesn't actually do anything but perform some IoC fluff)
    //User              - Can Save/Persist it's changes to the data store (EventStore)    
    //                  - Also loads it's Accounts?
    //Account           - Hmm....Some how will have to be able to raise generic events, but internally have provider specific implementations (e.g. for OAuth authentication and renewal)

    public class NewUserScenario
    {
        private CallWall.Web.User _user;
        private IAccount _account;
        private readonly IUserRepository _userRepository;

        public NewUserScenario()
        {
            _userRepository = new UserRepository();
        }

        public void GivenAnAnonUser()
        {
            _user = User.AnonUser;
        }

        public void WhenTheUserRegistersWithAnUnrecongisedAccount()
        {
            _account = new StubAccount();
            _account.CurrentSession.AuthorizedResources.Add("email");
            _account.CurrentSession.AuthorizedResources.Add("calendar");
            _user = _userRepository.RegisterNewUser(_account);
        }

        public void ThenAUserIsCreated()
        {
            var actualUser = _userRepository.FindByAccount(_account);
            Assert.IsNotNull(_user);
            Assert.IsNotNull(actualUser);
        }

        public void ThenAnAccountIsCreatedWithAnAccountIdentifier()
        {
            var actualUser = _userRepository.FindByAccount(_account);
            var actualAccount = actualUser.Accounts.Single();
            Assert.AreEqual(_account.Username, actualAccount.Username);
        }

        public void ThenAnAccountIsCreatedWithTheProvider() //(e.g. gmail/twitter/etc....)
        {
            var actualUser = _userRepository.FindByAccount(_account);
            var actualAccount = actualUser.Accounts.Single();
            Assert.AreEqual(_account.Provider, actualAccount.Provider);
        }

        public void ThenAnAccountIsCreatedWithPermissionsMappedToProviderScopes()
        //With permission key set and the mapping to the provider's OAuth Scopes e.g. {Key: "CallWall.Communications", Value: "https://mail.google.com/"}
        {
            var actualUser = _userRepository.FindByAccount(_account);
            var actualAccount = actualUser.Accounts.Single();
            CollectionAssert.AreEqual(_account.CurrentSession.AuthorizedResources, actualAccount.CurrentSession.AuthorizedResources);
        }

        public void ThenAnAccountContactRefreshCommandIsIssuedForTheAccount()
        {
            Assert.Inconclusive();
        }
    }

    public class UserRepository : IUserRepository
    {
        public User RegisterNewUser(IAccount account)
        {
            throw new NotImplementedException();
        }

        public User FindByAccount(IAccount account)
        {
            throw new NotImplementedException();
        }
    }

    internal interface IUserRepository
    {
        User RegisterNewUser(IAccount account);
        User FindByAccount(IAccount account);
    }

    public class StubAccount : IAccount
    {
        private readonly ISession _currentSession = new StubSession();

        public string Provider { get { return "TestProvider"; } }

        public string Username { get { return "Test.User@email.com"; } }

        public string DisplayName { get { return "Test User"; } }

        public ISession CurrentSession { get { return _currentSession; } }
    }

    public class StubSession : ISession
    {
        private readonly ISet<string> _authorizedResources;

        public StubSession(params string[] authorizedResources)
        {
            _authorizedResources = new HashSet<string>(authorizedResources);
        }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTimeOffset Expires { get; set; }

        public bool HasExpired()
        {
            return DateTimeOffset.Now > Expires;
        }

        public ISet<string> AuthorizedResources { get { return _authorizedResources; } }

        public string Serialize()
        {
            throw new NotImplementedException();
        }
    }

    //UserLogin
    //Adding extra accounts
    //Removing accounts
    //Account revoked
    //Merging users


    /*Port any sensible changes from PracticalRx End-2-end back into CallWall

Create set of Acceptance tests to document the expected behaviour

[Acceptance tests]
GIVEN an anonymous user
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
		
		
GIVEN a logged in user (i.e. a UserId) with a single account 
	WHEN they request their contacts [From a version/eventId]
		THEN they are returned a subscription to contactUpdates
		THEN they are pushed the contact events loaded in the data store
		
	WHEN user registers an additional account
		THEN an Account is created
			With an Account identifier
			With the Provider (gmail/twitter/etc....)
			With permission key set and the mapping to the provider's OAuth Scopes e.g. {Key: "CallWall.Communications", Value: "https://mail.google.com/"}
		THEN the new Account is associated to the current User
		THEN an AccountContact refresh command is issued for the Account
		
GIVEN a logged in user with a contact updates subscription
	WHEN a contact is added to the data store for the user's account
		THEN they are pushed the contact added event
	WHEN a contact is updated in the data store for the user's account
		THEN they are pushed the contact update event
	WHEN a contact is removed from the data store for the user's account
		THEN they are pushed the contact removal event
	
		
	





GIVEN logged in user removes a registered account
GIVEN logged in user registers an account that is already registered
GIVEN ?? WHEN Account has been revoked from Provider's side

GIVEN a registered account
	WHEN a contact refresh is issued
		THEN new contacts are stored as Contact added events
		THEN updated contacts are stored as Contact updated events
		THEN missing contacts are stored as Contact removal events
     
GIVEN logged in users
     Can request a list of current registered accounts and their providers
     Can request a list of all providers available to register with
     
     
     */
}
