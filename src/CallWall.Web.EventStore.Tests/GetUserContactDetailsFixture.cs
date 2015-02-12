using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
    [Story(Title = "Get User contacts by key",
        AsA = "As an end user",
        IWant = "I want to be able to fetch my contacts by a set of keys",
        SoThat = "So that I can see their details with just a public key like a phone number or email address")]
    //[Timeout(1000)]
    public class GetUserContactDetailsFixture
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
        public void Find_single_contact_by_key()
        {
            //Create user
            //Load an account contact into ES
            //--> should flow into UserContacts
            // get UserContact by accountContact key

            new FetchUserContactProfileScenario(_eventStoreClient)
                .Given(s => s.Given_a_ContactSynchronizationService())
                .When(s => s.When_a_user_registers_and_triggers_an_AccountRefresh())
                .Then(s => s.Then_contacts_are_available_by_key())
                .BDDfy();

        }

        public class FetchUserContactProfileScenario : IDisposable
        {
            private readonly UserRepository _userRepository;
            private readonly IUserContactRepository _userContactRepository;
            private readonly UserContactSynchronizationService _userContactSynchronizationService;
            private readonly IAccount _account;
            private readonly AccountContactSynchronizationService _accountContactSynchronizationService;
            private readonly IObservable<IAccountContactSummary> _expectedFeed;

            public FetchUserContactProfileScenario(IEventStoreClient eventStoreClient)
            {
                _userContactRepository = new UserContactRepository(eventStoreClient, new ConsoleLoggerFactory());
                var accountFactory = new AccountFactory();
                var accountContactRefresher = new AccountContactRefresher(eventStoreClient);
                _userRepository = new UserRepository(eventStoreClient, new ConsoleLoggerFactory(), accountFactory, accountContactRefresher);
                _account = CreateAccount();
                _expectedFeed = CreateStubFeed(_account);
                _accountContactSynchronizationService = CreateAccountContactSynchronizationService(eventStoreClient, _account, _expectedFeed);
                _userContactSynchronizationService = new UserContactSynchronizationService(eventStoreClient, new ConsoleLoggerFactory());
            }

            public User User { get; set; }

            public async Task Given_a_ContactSynchronizationService()
            {
                await Task.WhenAll(
                    _accountContactSynchronizationService.Run(),
                    _userContactSynchronizationService.Run(),
                    _userRepository.Run());
            }

            public async Task When_a_user_registers_and_triggers_an_AccountRefresh()
            {
                User = await _userRepository.RegisterNewUser(_account, Guid.NewGuid());
            }

            public async Task Then_contacts_are_available_by_key()
            {
                await WaitForContactsToBeLoaded();

                var actual = await _userContactRepository.GetContactDetails(User, new[] { "alex.adams@mail.com" })
                    .Take(1)
                    .Timeout(TimeSpan.FromSeconds(5))
                    .ToTask();
                Trace.WriteLine(actual);
                Assert.IsNotNull(actual);
                Assert.AreEqual("Alex Adams", actual.Title);
                Assert.AreEqual(actual.Handles.Count(), 1);
                Assert.AreEqual(actual.Handles.Single().Handle, "alex.adams@mail.com");
                Assert.AreEqual(actual.Handles.Single().HandleType, "Email");
                Assert.AreEqual(actual.Handles.Single().Qualifier, "home");
            }

            private async Task WaitForContactsToBeLoaded()
            {
                var expected = await _expectedFeed.ToList().FirstAsync();

                Trace.WriteLine("Expecting " + expected.Count + " values");

                var user = await _userRepository.Login(_account);
                Trace.WriteLine("Account logged in");

                var contacts = await _userContactRepository.GetContactSummariesFrom(user, 0)
                    .Do(u => Trace.WriteLine("GetContactSummariesFrom(user).OnNext()"))
                    .Select(evt => evt.Value)
                    .Take(expected.Count)
                    .ToList()
                    .FirstAsync();

                Trace.WriteLine("Received " + contacts.Count + " values");
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

            #region Factory methods

            private static AccountContactSynchronizationService CreateAccountContactSynchronizationService(
                IEventStoreClient eventStoreClient, IAccount account, IObservable<IAccountContactSummary> expectedFeed)
            {
                var dummyAccountContactProvider = CreateAccountContactProvider(account, expectedFeed);
                var accountContactsFactory = new AccountContactsFactory(eventStoreClient,
                    new ConsoleLoggerFactory(),
                    new[] { dummyAccountContactProvider });
                var accountContactSynchronizationService = new AccountContactSynchronizationService(eventStoreClient,
                    new ConsoleLoggerFactory(),
                    accountContactsFactory);
                return accountContactSynchronizationService;
            }

            private static IAccount CreateAccount()
            {
                var account = new StubAccount();
                account.CurrentSession.AuthorizedResources.Add("email");
                account.CurrentSession.AuthorizedResources.Add("calendar");
                return account;
            }

            private static IAccountContactProvider CreateAccountContactProvider(IAccount account, IObservable<IAccountContactSummary> expectedFeed)
            {
                var accountContactProvider = new StubAccountContactProvider(account.Provider, expectedFeed);
                return accountContactProvider;
            }


            private static IObservable<IAccountContactSummary> CreateStubFeed(IAccount account)
            {
                var accountId = account.AccountId;
                var provider = account.Provider;
                return
                    new[]
                    {
                        new StubContactSummary
                        {
                            AccountId = accountId,
                            Provider = provider,
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
                            },
                            Handles =
                            {
                                new ContactEmailAddress("alex.adams@mail.com", "home")
                            }
                        },
                        new StubContactSummary
                        {
                            AccountId = accountId,
                            Provider = provider,
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
                        },
                        new StubContactSummary
                        {
                            AccountId = accountId,
                            Provider = provider,
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
                        }
                    }.ToObservable();
            }

            #endregion

            public void Dispose()
            {
                _userRepository.Dispose();
                _userContactSynchronizationService.Dispose();
                _accountContactSynchronizationService.Dispose();
            }
        }

    }
}