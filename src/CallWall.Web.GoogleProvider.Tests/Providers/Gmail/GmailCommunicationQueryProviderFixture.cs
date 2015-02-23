using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Providers.Gmail;
using CallWall.Web.GoogleProvider.Providers.Gmail.Imap;
using Microsoft.Reactive.Testing;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using TestStack.BDDfy;

namespace CallWall.Web.GoogleProvider.Tests.Providers.Gmail
{
    //TODO: When Imap client fails to authenticate it should probably allow all other requests to continue, but get an error to the UI (so the user can see that a gmail lookup failed)

    [TestFixture]
    [Story(Title = "Gmail IMAP integration",
        AsA = "As a Gmail user",
        IWant = "I want to fetch the last emails for a given contact",
        SoThat = "So that I can have communication context for the contact")]
    [Timeout(1000)]
    public class GmailCommunicationQueryProviderFixture
    {
        [Test]
        public void Null_User_provided()
        {
            var imapClient = Substitute.For<IImapClient>();
            var logger = Substitute.For<ILoggerFactory>();
            var sut = new GmailCommunicationQueryProvider(() => imapClient, logger);

            var ex = Assert.Throws<ArgumentNullException>(() => sut.GetMessages(null, new string[0]));
            Assert.AreEqual("user", ex.ParamName);
        }
        [Test]
        public void Null_Contacts_provided()
        {
            var imapClient = Substitute.For<IImapClient>();
            var logger = Substitute.For<ILoggerFactory>();
            var sut = new GmailCommunicationQueryProvider(() => imapClient, logger);

            var ex = Assert.Throws<ArgumentNullException>(() => sut.GetMessages(CreateStubUser(), null));
            Assert.AreEqual("contactKeys", ex.ParamName);
        }

        [Test]
        public void Unauthtenticated_User()
        {
            new UnauthenticatedGmailUserScenario()
               .Given(s => s.Given_an_unauthenticated_user())
               .When(s => s.When_GetMessages_is_requested())
               .Then(s => s.Then_empty_sequence_is_returned())
               .BDDfy();
        }

        [Test]
        public void Empty_contacts_provided()
        {
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });
            new AuthenticatedGmailUserScenario()
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_contactKeys_are_empty())
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(0))
               .BDDfy();
        }

        [Test]
        public void UserWithSingleGmailAccount_single_contact()
        {
            var contact = "test@gmail.com";
            var expectedImapQuery = "\"test@gmail.com\"";
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });
            new AuthenticatedGmailUserScenario()
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_a_single_contactKey(contact))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(1))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(15))
               .BDDfy();
        }

        [Test]
        public void UserWithMultipleGmailAccount_single_contact()
        {
            var contact = "test@gmail.com";
            var expectedImapQuery = "\"test@gmail.com\"";
            var user = new User(Guid.NewGuid(), "Test", new[] { CreateGmailAccount(), CreateGmailAccount(), CreateGmailAccount() });
            new AuthenticatedGmailUserScenario()
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_a_single_contactKey(contact))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(user.Accounts.Count()))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(15))
               .BDDfy();
        }

        [Test]
        public void UserWithSingleGmailAccount_multiple_contact()
        {
            var contacts = new[]{"test@gmail.com", "bob@mail.com", "bob@work.com"};
            var expectedImapQuery = "\"test@gmail.com\" OR \"bob@mail.com\" OR \"bob@work.com\"";
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });
            new AuthenticatedGmailUserScenario()
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_multiple_contactKeys(contacts))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(1))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(15))
               .BDDfy();
        }

        [Test]
        public void UserWithMultipleGmailAccount_multiple_contact()
        {
            var contacts = new[] { "test@gmail.com", "bob@mail.com", "bob@work.com" };
            var expectedImapQuery = "\"test@gmail.com\" OR \"bob@mail.com\" OR \"bob@work.com\"";

            var user = new User(Guid.NewGuid(), "Test", new[] { CreateGmailAccount(), CreateGmailAccount(), CreateGmailAccount() });
            new AuthenticatedGmailUserScenario()
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_multiple_contactKeys(contacts))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(user.Accounts.Count()))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(15))
               .BDDfy();
        }

        [Test]
        public void Imap_client_fails_to_connect()
        {
            var contact = "test@gmail.com";
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });
            
            var logger = Substitute.For<ILoggerFactory>();
            var sut = new GmailCommunicationQueryProvider(()=>CreateImapClient(false, false, null), logger);

            var observer = CreateTestObserver();
            using (sut.GetMessages(user, new[] {contact}).Subscribe(observer))
            {
                Assert.AreEqual(1, observer.Messages.Count);

                Assert.AreEqual(0, observer.Messages[0].Time);
                Assert.AreEqual(NotificationKind.OnError, observer.Messages[0].Value.Kind);
                Assert.IsInstanceOf<IOException>(observer.Messages[0].Value.Exception);
                Assert.AreEqual("Failed to connect to Gmail IMAP server.", observer.Messages[0].Value.Exception.Message);
            }
        }
        

        [Test]
        public void Imap_client_fails_to_authenticate()
        {
            var contact = "test@gmail.com";
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });

            var logger = Substitute.For<ILoggerFactory>();
            var sut = new GmailCommunicationQueryProvider(() => CreateImapClient(true, false, null), logger);

            var observer = CreateTestObserver();
            using (sut.GetMessages(user, new[] { contact }).Subscribe(observer))
            {
                Assert.AreEqual(1, observer.Messages.Count);

                Assert.AreEqual(0, observer.Messages[0].Time);
                Assert.AreEqual(NotificationKind.OnError, observer.Messages[0].Value.Kind);
                Assert.IsInstanceOf<AuthenticationException>(observer.Messages[0].Value.Exception);
                Assert.AreEqual("Failed to authenticate for Gmail search.", observer.Messages[0].Value.Exception.Message);
            }
        }


        public class UnauthenticatedGmailUserScenario
        {
            private readonly GmailCommunicationQueryProvider _sut;
            private readonly IImapClient _imapClient;
            private User _user;
            private IObservable<IMessage> _messages;

            public UnauthenticatedGmailUserScenario()
            {
                _imapClient = Substitute.For<IImapClient>();
                var logger = Substitute.For<ILoggerFactory>();
                _sut = new GmailCommunicationQueryProvider(() => _imapClient, logger);
            }
            public void Given_an_unauthenticated_user()
            {
                _user = new User(Guid.NewGuid(), "Test User", Enumerable.Empty<IAccount>());
            }

            public void When_GetMessages_is_requested()
            {
                _messages = _sut.GetMessages(_user, new string[0]);
            }

            public void Then_empty_sequence_is_returned()
            {
                var observer = CreateTestObserver();
                using (_messages.Subscribe(observer))
                {
                    Assert.AreEqual(1, observer.Messages.Count);
                    Assert.AreEqual(ReactiveTest.OnCompleted<IMessage>(0), observer.Messages[0]);
                }
            }
        }

        public class AuthenticatedGmailUserScenario : IDisposable
        {
            private readonly GmailCommunicationQueryProvider _sut;
            private readonly List<IImapClient> _imapClients = new List<IImapClient>();
            private readonly List<ulong[]> _emailIds = new List<ulong[]>();
            private readonly ITestableObserver<IMessage> _observer = CreateTestObserver();
            private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();
            private User _user;
            private string[] _contackKeys;

            public AuthenticatedGmailUserScenario()
            {
                var logger = Substitute.For<ILoggerFactory>();
                _sut = new GmailCommunicationQueryProvider(ImapClientFactory, logger);
            }

            public void Given_an_authenticated_user(User user)
            {
                _user = user;
            }
            public void Given_contactKeys_are_empty()
            {
                _contackKeys = new string[0];
            }
            public void Given_a_single_contactKey(string contactKey)
            {
                _contackKeys = new string[1] { contactKey };
            }
            public void Given_multiple_contactKeys(string[] contacts)
            {
                _contackKeys = contacts;
            }

            public void When_GetMessages_requested()
            {
                _subscription.Disposable = _sut.GetMessages(_user, _contackKeys).Subscribe(_observer);
            }

            public void Then_new_imap_client_is_created(int expectedCreationCount)
            {
                Assert.AreEqual(expectedCreationCount, _imapClients.Count);
            }

            public void Then_imap_client_is_connected()
            {
                foreach (var imapClient in _imapClients)
                {
                    imapClient.Received().Connect("imap.gmail.com", 993);
                }
            }

            public void Then_imap_client_is_authenticated()
            {
                var accountClients = Enumerable.Zip(_imapClients, _user.Accounts, Tuple.Create);
                foreach (var t in accountClients)
                {
                    t.Item1.Received().Authenticate(t.Item2.AccountId, t.Item2.CurrentSession.AccessToken);
                }
            }

            public void Then_imap_client_folder_is_set_to(string folder)
            {
                foreach (var imapClient in _imapClients)
                {
                    imapClient.Received().SelectFolder(folder);
                }
            }

            public void Then_imap_client_is_searched_with(string expectedImapQuery)
            {
                foreach (var imapClient in _imapClients)
                {
                    imapClient.Received().FindEmailIds(expectedImapQuery);
                }
            }

            public void Then_imap_client_gets_most_recent_emails(int topCount)
            {
                var expectedEmailHandles = _user.Accounts
                    .SelectMany(acc => acc.Handles)
                    .Where(h => h.HandleType == ContactHandleTypes.Email)
                    .Select(h => h.Handle)
                    .ToArray();
                var pairs = Enumerable.Zip(_imapClients, _emailIds, Tuple.Create);
                foreach (var pair in pairs)
                {
                    var imapClient = pair.Item1;
                    var expectedEmailIds = pair.Item2.Reverse().Take(topCount);
                    imapClient.Received().FetchEmailSummaries(
                        Arg.Is<IEnumerable<ulong>>(emailIds=>emailIds.SequenceEqual(expectedEmailIds)), 
                        Arg.Is<IEnumerable<string>>(fromAddresses => fromAddresses.SequenceEqual(expectedEmailHandles)));
                }
            }

            private IImapClient ImapClientFactory()
            {
                var emailIds = GenerateRandomLongs();
                var imapClient = CreateImapClient(true, true, emailIds);
                _imapClients.Add(imapClient);
                _emailIds.Add(emailIds);                
                return imapClient;
            }

            private static ulong[] GenerateRandomLongs()
            {
                var rnd = new Random();
                return Enumerable.Range(0, 20).Select(_ => (ulong) rnd.Next())
                    .OrderBy(x => x)
                    .ToArray();
            }

            public void Dispose()
            {
                _subscription.Dispose();
            }

        }

        private static User CreateStubUser()
        {
            return new User(Guid.NewGuid(), "Test User", Enumerable.Empty<IAccount>());
        }

        private static IAccount CreateGmailAccount()
        {
            var accountId = Guid.NewGuid().ToString();
            var accessToken = Guid.NewGuid().ToString();
            var resources = new HashSet<string> { ResourceScope.Gmail.Resource };
            var account = Substitute.For<IAccount>();
            account.AccountId.Returns(accountId);
            account.Provider.Returns(Constants.ProviderName);
            account.CurrentSession.HasExpired().Returns(false);
            account.CurrentSession.AuthorizedResources.Returns(resources);
            account.CurrentSession.AccessToken.Returns(accessToken);

            return account;
        }

        private static ITestableObserver<IMessage> CreateTestObserver()
        {
            var observer = new TestScheduler().CreateObserver<IMessage>();
            return observer;
        }

        private static IImapClient CreateImapClient(bool canConnect, bool canAuthenticate, ulong[] emailIds)
        {
            var imapClient = Substitute.For<IImapClient>();
            imapClient.Connect("imap.gmail.com", 993).Returns(Observable.Return(canConnect));
            imapClient.Authenticate(Arg.Any<string>(), Arg.Any<string>()).Returns(Observable.Return(canAuthenticate));
            imapClient.FindEmailIds(Arg.Any<string>()).Returns(Observable.Return(emailIds));
            return imapClient;
        }
    }

    //The Imap stuff needs to tests that we
    //  can search by contact handle (multiple)
    //  can get date, subject, direction, content snippet & deeplink (from threadId)
}
