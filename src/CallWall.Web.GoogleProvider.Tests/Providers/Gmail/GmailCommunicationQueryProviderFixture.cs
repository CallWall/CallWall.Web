using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Authentication;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Providers.Gmail;
using CallWall.Web.GoogleProvider.Providers.Gmail.Imap;
using Microsoft.Reactive.Testing;
using NSubstitute;
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
        private const int MaxMessages = 10;
        private TestScheduler _testScheduler;

        [SetUp]
        public void SetUp()
        {
            _testScheduler = new TestScheduler();
        }

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
            new UnauthenticatedGmailUserScenario(_testScheduler)
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
            new AuthenticatedGmailUserScenario(_testScheduler)
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_contactKeys_are_empty())
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(0))
               .BDDfy();
        }

        [Test]
        public void Imap_client_fails_to_connect()
        {
            var contact = "test@gmail.com";
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });

            var logger = Substitute.For<ILoggerFactory>();
            var sut = new GmailCommunicationQueryProvider(() => CreateImapClient(false, false, null), logger);

            var observer = _testScheduler.CreateObserver<IMessage>();
            using (sut.GetMessages(user, new[] { contact }).Subscribe(observer))
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

            var observer = _testScheduler.CreateObserver<IMessage>();
            using (sut.GetMessages(user, new[] { contact }).Subscribe(observer))
            {
                Assert.AreEqual(1, observer.Messages.Count);

                Assert.AreEqual(0, observer.Messages[0].Time);
                Assert.AreEqual(NotificationKind.OnError, observer.Messages[0].Value.Kind);
                Assert.IsInstanceOf<AuthenticationException>(observer.Messages[0].Value.Exception);
                Assert.AreEqual("Failed to authenticate for Gmail search.", observer.Messages[0].Value.Exception.Message);
            }
        }

        [Test]
        public void UserWithSingleGmailAccount_single_contact()
        {
            var contact = "test@gmail.com";
            var expectedImapQuery = "\"test@gmail.com\"";
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });
            var expectedMessages = GenerateReturnedEmails(_testScheduler);

            new AuthenticatedGmailUserScenario(_testScheduler)
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_a_single_contactKey(contact))
               .And(s => s.Given_imap_client_will_return(expectedMessages))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(1))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(expectedMessages.Messages.Take(MaxMessages).Completes()))
               .BDDfy();
        }

        [Test]
        public void UserWithMultipleGmailAccount_single_contact()
        {
            var contact = "test@gmail.com";
            var expectedImapQuery = "\"test@gmail.com\"";
            
            var user = new User(Guid.NewGuid(), "Test", new[] { CreateGmailAccount(), CreateGmailAccount(), CreateGmailAccount() });
            var returnedSequences = new []{ GenerateReturnedEmails(_testScheduler), GenerateReturnedEmails(_testScheduler), GenerateReturnedEmails(_testScheduler)};
            var expectedMessages = returnedSequences.SelectMany(x => x.Messages.Take(MaxMessages)).Completes();
            new AuthenticatedGmailUserScenario(_testScheduler)
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_a_single_contactKey(contact))
               .And(s => s.Given_imap_client_will_return(returnedSequences))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(user.Accounts.Count()))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(expectedMessages))
               .BDDfy();
        }

        [Test]
        public void UserWithSingleGmailAccount_multiple_contact()
        {
            var contacts = new[]{"test@gmail.com", "bob@mail.com", "bob@work.com"};
            var expectedImapQuery = "\"test@gmail.com\" OR \"bob@mail.com\" OR \"bob@work.com\"";
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });
            var expectedMessages = GenerateReturnedEmails(_testScheduler);
            new AuthenticatedGmailUserScenario(_testScheduler)
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_multiple_contactKeys(contacts))
               .And(s => s.Given_imap_client_will_return(expectedMessages))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(1))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(expectedMessages.Messages.Take(MaxMessages).Completes()))
               .BDDfy();
        }

        [Test]
        public void UserWithMultipleGmailAccount_multiple_contact()
        {
            var contacts = new[] { "test@gmail.com", "bob@mail.com", "bob@work.com" };
            var expectedImapQuery = "\"test@gmail.com\" OR \"bob@mail.com\" OR \"bob@work.com\"";

            var user = new User(Guid.NewGuid(), "Test", new[] { CreateGmailAccount(), CreateGmailAccount(), CreateGmailAccount() });
            var returnedSequences = new[] { GenerateReturnedEmails(_testScheduler), GenerateReturnedEmails(_testScheduler), GenerateReturnedEmails(_testScheduler) };
            var expectedMessages = returnedSequences.SelectMany(x => x.Messages.Take(MaxMessages)).Completes();
            
            new AuthenticatedGmailUserScenario(_testScheduler)
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_multiple_contactKeys(contacts))
               .And(s => s.Given_imap_client_will_return(returnedSequences))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(user.Accounts.Count()))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(expectedMessages))
               .BDDfy();
        }

        [Test]
        public void UserWithSingleGmailAccount_distinct_threads()
        {
            var contact = "test@gmail.com";
            var expectedImapQuery = "\"test@gmail.com\"";
            var account = CreateGmailAccount();
            var user = new User(Guid.NewGuid(), "Test", new[] { account });

            var messages = new[]{
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=14" }), 
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=20" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=19" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=20" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=18" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=17" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=16" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=15" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=14" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=13" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=12" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=11" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=10" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=9" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=8" }),
            };
            //Distinct by deeplink, top 10.
            var expectedMessages = messages.Distinct(rnm => rnm.Value.Value.DeepLink)
                .Take(MaxMessages)
                .Completes()
                .ToArray();

            
            var returnedMessages = _testScheduler.CreateColdObservable(messages);

            new AuthenticatedGmailUserScenario(_testScheduler)
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_a_single_contactKey(contact))
               .And(s => s.Given_imap_client_will_return(returnedMessages))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(1))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(expectedMessages))
               .BDDfy();
        }

        [Test]
        public void UserWithMultipleGmailAccount_distinct_threads()
        {
            var contact = "test@gmail.com";
            var expectedImapQuery = "\"test@gmail.com\"";
            var user = new User(Guid.NewGuid(), "Test", new[] { CreateGmailAccount(), CreateGmailAccount(), CreateGmailAccount() });
            var acc1msgs = new[]{
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=14" }), 
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=20" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=19" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=20" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=18" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=17" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=16" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=15" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=14" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=13" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=12" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=11" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=10" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=9" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=1&threadId=8" }),
            };
            var acc1Seq = _testScheduler.CreateColdObservable(acc1msgs);
            var acc2Msgs = new[]{
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=14" }), 
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=20" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=19" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=20" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=18" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=17" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=16" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=15" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=14" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=13" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=12" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=11" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=10" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=9" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=2&threadId=8" }),
            };
            var acc2Seq = _testScheduler.CreateColdObservable(acc2Msgs);
            var acc3Msgs = new[]{
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=14" }), 
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=20" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=19" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=20" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=18" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=17" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=16" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=16" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=15" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=14" }),   //Duplicate
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=13" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=12" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=11" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=10" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=9" }),
                ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?acc=3&threadId=8" }),
            };
            var acc3Seq = _testScheduler.CreateColdObservable(acc3Msgs);

            var returnedSequences = new[] { acc1Seq, acc2Seq, acc3Seq };
            var expectedMessages = returnedSequences.SelectMany(x => x.Messages.Distinct(m=>m.Value.Value.DeepLink).Take(MaxMessages)).Completes();
            new AuthenticatedGmailUserScenario(_testScheduler)
               .Given(s => s.Given_an_authenticated_user(user))
               .And(s => s.Given_a_single_contactKey(contact))
               .And(s => s.Given_imap_client_will_return(returnedSequences))
               .When(s => s.When_GetMessages_requested())
               .Then(s => s.Then_new_imap_client_is_created(user.Accounts.Count()))
               .Then(s => s.Then_imap_client_is_connected())
               .Then(s => s.Then_imap_client_is_authenticated())
               .Then(s => s.Then_imap_client_folder_is_set_to("[Gmail]/All Mail"))
               .Then(s => s.Then_imap_client_is_searched_with(expectedImapQuery))
               .Then(s => s.Then_imap_client_gets_most_recent_emails(expectedMessages))
               .BDDfy();
        }



        public class UnauthenticatedGmailUserScenario
        {
            private readonly GmailCommunicationQueryProvider _sut;
            private readonly TestScheduler _testScheduler;
            private readonly IImapClient _imapClient;
            private User _user;
            private IObservable<IMessage> _messages;

            public UnauthenticatedGmailUserScenario(TestScheduler testScheduler)
            {
                _testScheduler = testScheduler;
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
                var observer = _testScheduler.CreateObserver<IMessage>();
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
            private readonly TestScheduler _testScheduler;
            private readonly List<IImapClient> _imapClients = new List<IImapClient>();
            private readonly List<ulong[]> _emailIds = new List<ulong[]>();
            private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();
            private readonly ITestableObserver<IMessage> _observer;
            private User _user;
            private string[] _contackKeys;

            public AuthenticatedGmailUserScenario(TestScheduler testScheduler)
            {
                _testScheduler = testScheduler;
                var logger = Substitute.For<ILoggerFactory>();
                _observer = _testScheduler.CreateObserver<IMessage>();
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

            public void Given_imap_client_will_return(params IObservable<IMessage>[] expectedMessages)
            {
                _expectedMessages = expectedMessages;
            }

            public void When_GetMessages_requested()
            {
                _subscription.Disposable = _sut.GetMessages(_user, _contackKeys).Subscribe(_observer);
                _testScheduler.Start();
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

            public void Then_imap_client_gets_most_recent_emails(IList<Recorded<Notification<IMessage>>> expectedMessages)
            {
                var expectedEmailHandles = _user.Accounts
                    .SelectMany(acc => acc.Handles)
                    .Where(h => h.HandleType == ContactHandleTypes.Email)
                    .Select(h=>h.Handle)
                    .ToList();

                var itemsByIndex = Enumerable.Range(0, _imapClients.Count)
                    .Select(i => new
                    {
                        ImapClient = _imapClients[i],
                        EmailIds = _emailIds[i],
                        AccountHandle = expectedEmailHandles[i]
                    });

                foreach (var item in itemsByIndex)
                {
                    var imapClient = item.ImapClient;
                    var expectedEmailIds = item.EmailIds.Reverse();
                    var expectedAccountHandle = item.AccountHandle;
                    imapClient.Received().FetchEmailSummaries(
                        Arg.Is<IEnumerable<ulong>>(emailIds=>emailIds.SequenceEqual(expectedEmailIds)),
                        Arg.Is<string>(fromAddress => string.Equals(fromAddress, expectedAccountHandle, StringComparison.OrdinalIgnoreCase)));
                }

                CollectionAssert.AreEqual(expectedMessages, _observer.Messages);
            }

            private IObservable<IMessage>[] _expectedMessages;

            private IImapClient ImapClientFactory()
            {
                var emailIds = GenerateRandomLongs();
                var imapClient = CreateImapClient(true, true, emailIds);
                var idx = _imapClients.Count;
                _imapClients.Add(imapClient);                
                _emailIds.Add(emailIds);
                var returnedEmails = _expectedMessages[idx];
                imapClient.FetchEmailSummaries(Arg.Any<IEnumerable<ulong>>(), Arg.Any<string>()).Returns(returnedEmails);
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
            var account = CreateGmailAccount();
            return new User(Guid.NewGuid(), "Test User", new[] { account });
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
            var handles = new[]
            {
                new ContactEmailAddress("Lee@mail.com", "main")
            };
            account.Handles.Returns(handles);

            return account;
        }

        private static ITestableObservable<IMessage> GenerateReturnedEmails(TestScheduler testScheduler)
        {
            var messages = Enumerable.Range(1, 20)
                .Select(i => ReactiveTest.OnNext<IMessage>(1, new StubMessage { DeepLink = "http://m.com/?threadId=" + i.ToString() }))
                .Reverse()
                .ToArray();
            return testScheduler.CreateColdObservable(messages);
        }

        private static IImapClient CreateImapClient(bool canConnect, bool canAuthenticate, ulong[] emailIds)
        {
            var imapClient = Substitute.For<IImapClient>();
            imapClient.Connect("imap.gmail.com", 993).Returns(Observable.Return(canConnect));
            imapClient.Authenticate(Arg.Any<string>(), Arg.Any<string>()).Returns(Observable.Return(canAuthenticate));
            imapClient.FindEmailIds(Arg.Any<string>()).Returns(Observable.Return(emailIds));
            return imapClient;
        }

        private class StubMessage : IMessage
        {
            public DateTimeOffset Timestamp { get; set; }
            public MessageDirection Direction { get; set; }
            public string Subject { get; set; }
            public string DeepLink { get; set; }
            string IMessage.Content { get { return null; } }
            IProviderDescription IMessage.Provider { get { return GmailProviderDescription.Instance; } }
            MessageType IMessage.MessageType { get { return MessageType.Email; } }

            public override string ToString()
            {
                return string.Format("StubMessage {{ DeepLink:'{0}' }}", DeepLink);
            }

            protected bool Equals(StubMessage other)
            {
                return string.Equals(DeepLink, other.DeepLink) && string.Equals(Subject, other.Subject) && Direction == other.Direction && Timestamp.Equals(other.Timestamp);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((StubMessage) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (DeepLink != null ? DeepLink.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (Subject != null ? Subject.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (int) Direction;
                    hashCode = (hashCode*397) ^ Timestamp.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(StubMessage left, StubMessage right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(StubMessage left, StubMessage right)
            {
                return !Equals(left, right);
            }
        }
    }

    public static class TestExtensions
    {
        public static IList<Recorded<Notification<T>>> Completes<T>(this IEnumerable<Recorded<Notification<T>>> source)
        {
            var result = source.ToList();
            var lastValue = result.LastOrDefault();
            var lastTime = lastValue == default(Recorded<Notification<T>>) ? 0 : lastValue.Time;
            result.Add(ReactiveTest.OnCompleted<T>(lastTime));
            return result;
        }

        public static IEnumerable<TValue> Distinct<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> selector)
        {
            var set = new HashSet<TKey>();
            return source.Where(x=>set.Add(selector(x)));
        }
    }
    //The Imap stuff needs to tests that we
    //  can search by contact handle (multiple)
    //  can get date, subject, direction, content snippet & deeplink (from threadId)
}
