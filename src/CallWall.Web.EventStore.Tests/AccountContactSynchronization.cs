using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Accounts;
using CallWall.Web.EventStore.Contacts;
using CallWall.Web.EventStore.Domain;
using CallWall.Web.EventStore.Tests.Doubles;
using CallWall.Web.EventStore.Users;
using CallWall.Web.Providers;
using NSubstitute;
using NUnit.Framework;
using TestStack.BDDfy;

namespace CallWall.Web.EventStore.Tests
{
    [TestFixture]
    [Story(Title = "Account Contact Synchronization",
        AsA = "As a system",
        IWant = "I want to keep a synchronized cache of user's contacts per account",
        SoThat = "So that I can present this data to the user quickly")]
    [Timeout(15000)]
    public class AccountContactSynchronization
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
               .Then(s => s.Then_Account_contacts_are_availble_from_User_contacts_feed())
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

        //TODO: Implement IDisposable.
        public class UserRegistrationAccountContactSynchronizationScenario
        {
            private readonly UserRepository _userRepository;
            private readonly IUserContactRepository _userContactRepository;
            private readonly IContactSynchronizationService _userContactSynchronizationService;
            private readonly IAccount _account;
            private readonly AccountContactSynchronizationService _accountContactSynchronizationService;
            private readonly StubFeed _expectedFeed;

            public UserRegistrationAccountContactSynchronizationScenario(IEventStoreClient eventStoreClient)
            {
                _userContactRepository = new UserContactRepository(eventStoreClient);
                _userRepository = new UserRepository(eventStoreClient, Substitute.For<IAccountContactRefresher>());
                _account = CreateAccount(_userRepository, eventStoreClient);
                _expectedFeed = CreateStubFeed(_account);
                _accountContactSynchronizationService = CreateAccountContactSynchronizationService(eventStoreClient, _account, _expectedFeed);
                _userContactSynchronizationService = new UserContactSynchronizationService(eventStoreClient);
            }

            public void Given_a_ContactSynchronizationService()
            {
                _accountContactSynchronizationService.Run();
                _userContactSynchronizationService.Run();
                _userRepository.Run();
            }

            public async Task When_a_user_registers_and_triggers_an_AccountRefresh()
            {
                await _userRepository.RegisterNewUser(_account, Guid.NewGuid());
            }
            
            public async Task Then_Account_contacts_are_availble_from_User_contacts_feed()
            {
                var expected = await _expectedFeed.Values.ToList().FirstAsync();
                Trace.WriteLine("Expecting " + expected.Count + " values");

                var user = await _account.Login();
                Trace.WriteLine("Account logged in");

                var contacts = await _userContactRepository.GetContactSummariesFrom(user, null)
                    .Do(u => Trace.WriteLine("GetContactSummariesFrom(user).OnNext()"))
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
                CollectionAssert.AreEqual(new[] { summary.PrimaryAvatar }, update.AddedAvatars);
                Assert.AreEqual( summary.Provider, update.AddedProviders.Single().ProviderName);
                Assert.AreEqual(summary.AccountId, update.AddedProviders.Single().AccountId);
                Assert.AreEqual(summary.ProviderId, update.AddedProviders.Single().ContactId);
                CollectionAssert.AreEqual(summary.Tags, update.AddedTags);
                Assert.AreEqual(summary.Title, update.NewTitle);
                
            }



            #region Factory methods

            private static AccountContactSynchronizationService CreateAccountContactSynchronizationService(
                IEventStoreClient eventStoreClient, IAccountData account, IFeed<IAccountContactSummary> expectedFeed)
            {
                var dummyAccountContactProvider = CreateAccountContactProvider(account, expectedFeed);
                var accountContactsFactory = new AccountContactsFactory(eventStoreClient,
                    new[] {dummyAccountContactProvider});
                var accountContactSynchronizationService = new AccountContactSynchronizationService(eventStoreClient,
                    accountContactsFactory);
                return accountContactSynchronizationService;
            }

            private static IAccount CreateAccount(IUserRepository userRepository,
                IEventStoreClient eventStoreClient)
            {
                var accountContactRefresher = new AccountContactRefresher(eventStoreClient);
                var account = new StubAccount(userRepository, accountContactRefresher);
                account.CurrentSession.AuthorizedResources.Add("email");
                account.CurrentSession.AuthorizedResources.Add("calendar");
                return account;
            }

            private static IAccountContactProvider CreateAccountContactProvider(IAccountData account, IFeed<IAccountContactSummary> expectedFeed)
            {
                var accountContactProvider = new StubAccountContactProvider(account.Provider, expectedFeed.Values, expectedFeed.TotalResults);
                return accountContactProvider;
            }


            private static StubFeed CreateStubFeed(IAccountData account)
            {
                var accountId = account.AccountId;
                var provider = account.Provider;
                var stubFeed = new StubFeed(3,
                    new[]
                    {
                        new StubContactSummary
                        {
                            AccountId = accountId,
                            Provider = provider,
                            ProviderId = Guid.NewGuid().ToString(),
                            Title = "Alex Adams",
                            PrimaryAvatar = "http://image.coms/AnonAvatar.png",
                            Tags =
                            {
                                "Work",
                                "Squash"
                            }
                        },
                        new StubContactSummary
                        {
                            AccountId = accountId,
                            Provider = provider,
                            ProviderId = Guid.NewGuid().ToString(),
                            Title = "Billy Bonds",
                            PrimaryAvatar = "http://image.coms/AnonAvatar.png",
                            Tags =
                            {
                                "Family"
                            }
                        },
                        new StubContactSummary
                        {
                            AccountId = accountId,
                            Provider = provider,
                            ProviderId = Guid.NewGuid().ToString(),
                            Title = "Charlie Cheese",
                            PrimaryAvatar = "http://image.coms/AnonAvatar.png",
                            Tags =
                            {
                                "Neighbour"
                            }
                        },
                    }.ToObservable());
                return stubFeed;
            }
            
            #endregion
        }
    }
}