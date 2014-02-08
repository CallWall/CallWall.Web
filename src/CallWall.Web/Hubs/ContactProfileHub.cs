using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CallWall.Web.Providers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactProfile")]
    public class ContactProfileHub : Hub
    {
        private readonly IEnumerable<IContactsProvider> _contactsProviders;
        private readonly ISessionProvider _sessionProvider;
        private readonly ILogger _logger;
        private readonly SerialDisposable _contactProfileSubscription = new SerialDisposable();

        public ContactProfileHub(IEnumerable<IContactsProvider> contactsProviders, ISessionProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            _contactsProviders = contactsProviders;
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void Subscribe(string[] contactKeys)
        {
            _logger.Debug("ContactProfile.Subscribe({0})", string.Join(",", contactKeys));

            var subscription = Observable.Interval(TimeSpan.FromSeconds(1))
               .Zip(Profiles(), (_, p) => p)
               .Subscribe(profile => Clients.Caller.OnNext(profile),
                          ex => Clients.Caller.OnError(ex),
                          () => Clients.Caller.OnCompleted());

            //var sessions = _sessionProvider.GetSessions(Context.User);
            //var subscription = _contactsProviders
            //                    .ToObservable()
            //                    .SelectMany(c => c.GetContactDetails(sessions))
            //                    .Log(_logger, "RequestContactProfile")
            //                    .Subscribe(contact => Clients.Caller.ReceiveContactProfile(contact),
            //                               ex => Clients.Caller.ReceiveError("Error receiving contact profile"),
            //                               () => Clients.Caller.ReceiveComplete());

            _contactProfileSubscription.Disposable = subscription;
        }

        private IEnumerable<object> Profiles()
        {
            yield return new
                {
                    title = "Lee HUB Campbell",
                    //fullName = "",
                    dateOfBirth = new DateTime(1979, 12, 25),
                    tags = new[] { "Family", "Dolphins", "London" },
                    organizations =
                        new[]
                            {
                                new ContactAssociation("Consultant", "Adaptive"),
                                new ContactAssociation("Triathlon", "Serpentine")
                            },
                    relationships =
                        new[] { new ContactAssociation("Wife", "Erynne"), new ContactAssociation("Brother", "Rhys") },
                    phoneNumbers =
                        new[]
                            {
                                new ContactAssociation("Mobile - UK", "07827743025"),
                                new ContactAssociation("Mobile - NZ", "021 254 3824")
                            },
                    emailAddresses =
                        new[]
                            {
                                new ContactAssociation("Home", "lee.ryan.campbell@gmail.com"),
                                new ContactAssociation("Work", "lee.campbell@callwall.com")
                            },
                };
            yield return new
                {
                    //title = "Lee Campbell",
                    fullName = "Mr. Lee Ryan Campbell",
                    dateOfBirth = new DateTime(1979, 12, 27),
                    tags = new[] { "Adaptive", "Serpentine", "ReactConf", "Amazon", "Turtle" },
                    organizations = new[] { new ContactAssociation("CEO", "CallWall") },
                    relationships = new[] { new ContactAssociation("CFO", "John Bell"), },
                };
        }

        public override Task OnDisconnected()
        {
            _contactProfileSubscription.Dispose();
            return base.OnDisconnected();
        }

        private class ContactAssociation
        {
            public ContactAssociation(string name, string association)
            {
                this.name = name;
                this.association = association;
            }

            public string name { get; private set; }

            public string association { get; private set; }
        }
    }
}