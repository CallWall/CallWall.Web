using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Accounts;
using CallWall.Web.EventStore.Contacts;
using CallWall.Web.EventStore.Tests.Doubles;
using CallWall.Web.EventStore.Users;
using CallWall.Web.Providers;
using NUnit.Framework;
using TestStack.BDDfy;

namespace CallWall.Web.EventStore.Tests
{
    [TestFixture]
    [Story(Title = "Account Contact Synchronization",
        AsA = "As a system",
        IWant = "I want to keep a synchronized cache of user's contacts per account",
        SoThat = "So that I can present this data to the user quickly")]
    [Timeout(20000)]
    public class AccountContactSynchronizationFixture
    {
        #region Setup/TearDown

        private EmbeddedEventStoreConnectionFactory _connectionFactory;
        private EventStoreClient _eventStoreClient;

        [SetUp]
        public void SetUp()
        {
            _connectionFactory = new EmbeddedEventStoreConnectionFactory();
            _eventStoreClient = new EventStoreClient(_connectionFactory, new ConsoleLoggerFactory());
        }

        [TearDown]
        public void TearDown()
        {
            _eventStoreClient.Dispose();
            _connectionFactory.Dispose();
        }

        #endregion

        //First account (registered)
        //New User Account (registered)
        //Additional Account (reg)
        //Login
        //Restart

        [Test]
        public void InitialUserRegistration()
        {
            new UserRegistrationAccountContactSynchronizationScenario(_eventStoreClient)
               .Given(s => s.Given_a_ContactSynchronizationService())
               .When(s => s.When_a_user_registers_and_triggers_an_AccountRefresh())
               .Then(s => s.Then_Account_contacts_are_available_from_User_contacts_feed())
               .TearDownWith(s => s.Dispose())
               .BDDfy();
        }

        //[Test]
        //public void Dont_reprocess_AccountRefresh_events_after_system_restart()
        //{
        //    Assert.Inconclusive("TBD");
        //    //new UserRegistrationAccountContactSynchronizationScenario(_connectionFactory)
        //    //   .Given(s => s.Given_a_ContactSynchronizationService())
        //    //   .And(s => s.And_processed_AccountRefresh_events_exist())
        //    //   .Then(s => s.The_events_are_not_reprocessed())
        //    //   .BDDfy();
        //}

        //[Test]
        //public void Process_unhandled_AccountRefresh_events_after_system_restart()
        //{
        //    Assert.Inconclusive("TBD");
        //    //new UserRegistrationAccountContactSynchronizationScenario(_connectionFactory)
        //    //   .Given(s => s.Given_a_ContactSynchronizationService())
        //    //   .And(s => s.And_processed_AccountRefresh_events_exist())
        //    //   .Then(s => s.The_events_are_not_reprocessed())
        //    //   .BDDfy();
        //}

        [Test]
        public void UserAddsAccount()
        {
            new UserAddsAccountAndContactsSynchronizeScenario(_eventStoreClient)
               .Given(s => s.Given_a_ContactSynchronizationService())
               .And(s => s.Given_a_signed_in_user())
               .When(s => s.When_a_user_registers_an_account())
               .Then(s => s.Then_Account_contacts_are_available_from_User_contacts_feed())
               .TearDownWith(s => s.Dispose())
               .BDDfy();
        }
        //TODO: Add a test to ensure that when an exception is thrown, that ES gives a better error and logs. -LC

        public class UserRegistrationAccountContactSynchronizationScenario : IDisposable
        {
            private readonly UserRepository _userRepository;
            private readonly IContactFeedRepository _contactFeedRepository;
            private readonly UserContactSynchronizationService _userContactSynchronizationService;
            private readonly IAccount _account;
            private readonly AccountContactSynchronizationService _accountContactSynchronizationService;
            private readonly IObservable<IAccountContactSummary> _expectedFeed;
            private User _user;

            public UserRegistrationAccountContactSynchronizationScenario(IEventStoreClient eventStoreClient)
            {
                _contactFeedRepository = new EventStoreContactFeedRepository(eventStoreClient, new ConsoleLoggerFactory());
                var accountFactory = new AccountFactory();
                var accountContactRefresher = new AccountContactRefresher(eventStoreClient);
                _userRepository = new UserRepository(eventStoreClient, new ConsoleLoggerFactory(), accountFactory, accountContactRefresher);
                _account = CreateAccount("Main@email.com");
                _expectedFeed = new[] { Alex(_account), Billy(_account), Charlie(_account) }.ToObservable();
                var accContactProviders = new[] { CreateAccountContactProvider(_account, _expectedFeed) };
                _accountContactSynchronizationService = CreateAccountContactSynchronizationService(eventStoreClient, accContactProviders);
                _userContactSynchronizationService = new UserContactSynchronizationService(eventStoreClient, new ConsoleLoggerFactory());
            }

            public async Task Given_a_ContactSynchronizationService()
            {
                await Task.WhenAll(
                    _accountContactSynchronizationService.Run(),
                    _userContactSynchronizationService.Run(),
                    _userRepository.Run());
            }

            public async Task When_a_user_registers_and_triggers_an_AccountRefresh()
            {
                _user = await _userRepository.Login(_account);
                Trace.WriteLine("Account logged in");
            }

            public async Task Then_Account_contacts_are_available_from_User_contacts_feed()
            {
                var expected = await _expectedFeed.ToList().FirstAsync();

                Trace.WriteLine("Expecting " + expected.Count + " values");

                var contacts = await _contactFeedRepository.GetContactUpdates(_user, 0)
                    .Do(u => Trace.WriteLine("GetContactSummariesFrom(user).OnNext()"))
                    .Select(evt => evt.Value)
                    .Take(expected.Count)
                    .ToList()
                    .FirstAsync();

                Trace.WriteLine("Received " + contacts.Count + " values");

                Assert.IsNotNull(contacts);
                Assert.AreEqual(expected.Count, contacts.Count);

                for (int i = 0; i < expected.Count; i++)
                {
                    IsSummaryEqualToUpdate(expected[i], contacts[i]);
                    Assert.AreEqual(1, contacts[i].Version);
                }
            }

            private static void IsSummaryEqualToUpdate(IAccountContactSummary summary, ContactAggregateUpdate update)
            {
                CollectionAssert.AreEqual(summary.AvatarUris, update.AddedAvatars);
                Assert.AreEqual(summary.Provider, update.AddedProviders.Single().ProviderName);
                Assert.AreEqual(summary.AccountId, update.AddedProviders.Single().AccountId);
                Assert.AreEqual(summary.ProviderId, update.AddedProviders.Single().ContactId);
                CollectionAssert.AreEqual(summary.Tags, update.AddedTags);
                Assert.AreEqual(summary.Title, update.NewTitle);
            }

            public void Dispose()
            {
                _userRepository.Dispose();
                _userContactSynchronizationService.Dispose();
                _accountContactSynchronizationService.Dispose();
            }
        }

        public class UserAddsAccountAndContactsSynchronizeScenario : IDisposable
        {
            private readonly UserRepository _userRepository;
            private readonly IContactFeedRepository _contactFeedRepository;
            private readonly UserContactSynchronizationService _userContactSynchronizationService;
            private readonly IAccount _initialAccount;
            private readonly IAccount _newAccount;
            private readonly AccountContactSynchronizationService _accountContactSynchronizationService;
            private readonly IObservable<IAccountContactSummary> _initialAccountContactFeed;
            private readonly IObservable<IAccountContactSummary> _newAccountContactFeed;
            private User _initialUser;
            private User _updatedUser;

            public UserAddsAccountAndContactsSynchronizeScenario(IEventStoreClient eventStoreClient)
            {
                _contactFeedRepository = new EventStoreContactFeedRepository(eventStoreClient, new ConsoleLoggerFactory());
                var accountFactory = new AccountFactory();
                var accountContactRefresher = new AccountContactRefresher(eventStoreClient);
                _userRepository = new UserRepository(eventStoreClient, new ConsoleLoggerFactory(), accountFactory, accountContactRefresher);
                _initialAccount = CreateAccount("main");
                _initialAccountContactFeed = new[] { Alex(_initialAccount), Billy(_initialAccount), Charlie(_initialAccount) }.ToObservable();

                _newAccount = CreateAccount("other", "OtherProvider");
                _newAccountContactFeed = new[] { Alex(_newAccount), Derek(_newAccount) }.ToObservable();

                var accContactProviders = new[]
                {
                    CreateAccountContactProvider(_initialAccount, _initialAccountContactFeed),
                    CreateAccountContactProvider(_newAccount, _newAccountContactFeed),
                };

                _accountContactSynchronizationService = CreateAccountContactSynchronizationService(eventStoreClient, accContactProviders);
                _userContactSynchronizationService = new UserContactSynchronizationService(eventStoreClient, new ConsoleLoggerFactory());
            }

            public async Task Given_a_ContactSynchronizationService()
            {
                await Task.WhenAll(
                    _accountContactSynchronizationService.Run(),
                    _userContactSynchronizationService.Run(),
                    _userRepository.Run());
            }

            public async Task Given_a_signed_in_user()
            {
                _initialUser = await _userRepository.Login(_initialAccount);
                Trace.WriteLine("User logged in with Account " + _initialAccount.AccountId);
            }

            public async Task When_a_user_registers_an_account()
            {
                _updatedUser = await _userRepository.RegisterAccount(_initialUser.Id, _newAccount);
                Trace.WriteLine("User registered additional account of " + _newAccount.AccountId);
            }

            public async Task Then_Account_contacts_are_available_from_User_contacts_feed()
            {
                var initialAccountContacts = await _initialAccountContactFeed.ToList().FirstAsync();
                var additionalAccountContacts = await _newAccountContactFeed.ToList().FirstAsync();
                var expectedCount = initialAccountContacts.Count + additionalAccountContacts.Count;
                Trace.WriteLine("Expecting " + expectedCount + " values");

                var contacts = await _contactFeedRepository.GetContactUpdates(_updatedUser, 0)
                    .Do(u => Trace.WriteLine("GetContactSummariesFrom(user).OnNext()"))
                    .Select(evt => evt.Value)
                    .Take(expectedCount)
                    .ToList()
                    .FirstAsync();

                Trace.WriteLine("Received " + contacts.Count + " values");

                Assert.IsNotNull(contacts);
                Assert.AreEqual(expectedCount, contacts.Count);

                var i = 0;
                for (i = 0; i < initialAccountContacts.Count; i++)
                {
                    IsSummaryContainedByUpdate(initialAccountContacts[i], contacts[i]);
                    Assert.AreEqual(1, contacts[i].Version);
                }
            }

            private static void IsSummaryContainedByUpdate(IAccountContactSummary summary, ContactAggregateUpdate update)
            {
                CollectionAssert.AreEqual(summary.AvatarUris, update.AddedAvatars);
                Assert.AreEqual(summary.Provider, update.AddedProviders.Single().ProviderName);
                Assert.AreEqual(summary.AccountId, update.AddedProviders.Single().AccountId);
                Assert.AreEqual(summary.ProviderId, update.AddedProviders.Single().ContactId);
                CollectionAssert.AreEqual(summary.Tags, update.AddedTags);
                Assert.AreEqual(summary.Title, update.NewTitle);
            }

            public void Dispose()
            {
                _userRepository.Dispose();
                _userContactSynchronizationService.Dispose();
                _accountContactSynchronizationService.Dispose();
            }
        }

        #region Factory methods
        private static StubContactSummary Alex(IAccount account)
        {
            return new StubContactSummary
            {
                AccountId = account.AccountId,
                Provider = account.Provider,
                ProviderId = Guid.NewGuid().ToString(),
                Title = "Alex Adams",
                AvatarUris =
                {
                    "http://image.coms/AnonAvatar.png"
                },
                Tags =
                {
                    "Work",
                    "Squash"
                }
            };
        }
        private static StubContactSummary Billy(IAccount account)
        {
            return new StubContactSummary
            {
                AccountId = account.AccountId,
                Provider = account.Provider,
                ProviderId = Guid.NewGuid().ToString(),
                Title = "Billy Bonds",
                AvatarUris =
                {
                    "http://image.coms/AnonAvatar.png"
                },
                Tags =
                {
                    "Family"
                }
            };
        }
        private static StubContactSummary Charlie(IAccount account)
        {
            return new StubContactSummary
            {
                AccountId = account.AccountId,
                Provider = account.Provider,
                ProviderId = Guid.NewGuid().ToString(),
                Title = "Charlie Cheese",
                AvatarUris =
                {
                    "http://image.coms/AnonAvatar.png"
                },
                Tags =
                {
                    "Neighbour"
                }
            };
        }
        private static StubContactSummary Derek(IAccount account)
        {
            return new StubContactSummary
            {
                AccountId = account.AccountId,
                Provider = account.Provider,
                ProviderId = Guid.NewGuid().ToString(),
                Title = "Derek Doorknob",
                AvatarUris =
                {
                    "http://image.coms/AnonAvatar.png"
                },
                Tags =
                {
                    "Project Manager"
                }
            };
        }

        private static IAccountContactProvider CreateAccountContactProvider(IAccount account, IObservable<IAccountContactSummary> expectedFeed)
        {
            var accountContactProvider = new StubAccountContactProvider(account.Provider, expectedFeed);
            return accountContactProvider;
        }

        private static AccountContactSynchronizationService CreateAccountContactSynchronizationService(
            IEventStoreClient eventStoreClient, IEnumerable<IAccountContactProvider> accountContactProviders)
        {
            var accountContactsFactory = new AccountContactsFactory(eventStoreClient,
                new ConsoleLoggerFactory(),
                accountContactProviders);
            var accountContactSynchronizationService = new AccountContactSynchronizationService(eventStoreClient,
                new ConsoleLoggerFactory(),
                accountContactsFactory);
            return accountContactSynchronizationService;
        }

        private static IAccount CreateAccount(string accountId, string provider = "TestProvider")
        {
            var account = new StubAccount
            {
                AccountId = accountId,
                Provider = provider
            };
            account.CurrentSession.AuthorizedResources.Add("email");
            account.CurrentSession.AuthorizedResources.Add("calendar");
            return account;
        }

        #endregion

    }
}