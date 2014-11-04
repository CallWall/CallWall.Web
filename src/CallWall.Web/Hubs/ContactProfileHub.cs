using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CallWall.Web.Providers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace CallWall.Web.Hubs
{
    [HubName("contactProfile")]
    public class ContactProfileHub : Hub
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly IEnumerable<IAccountContactProvider> _contactsProviders;
        private readonly ILoginProvider _loginProvider;
        private readonly ILogger _logger;

        public ContactProfileHub(IEnumerable<IAccountContactProvider> contactsProviders, ILoginProvider loginProvider, ILoggerFactory loggerFactory)
        {
            _contactsProviders = contactsProviders.ToArray();
            _loginProvider = loginProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactProfileHub.ctor(contactsProviders:{0})", string.Join(",", _contactsProviders.Select(cp=>cp.GetType().Name)));
        }

        public async Task Subscribe(string[] contactKeys)
        {
            Debug.Print("ContactProfileHub.Subscribe(...)");


            //TODO: The first thing we should do is get what we already have on the contact from the EventStore. I suppose we can just add an EventStore implementation of the GetContactDetails


            var user = await _loginProvider.GetUser(Context.User.UserId());
            var subscription = _contactsProviders
                                .ToObservable()
                                .SelectMany(c => c.GetContactDetails(user, contactKeys))
                                .Log(_logger, "GetContactDetails")
                                .Subscribe(contact => Clients.Caller.OnNext(contact),
                                           ex => Clients.Caller.OnError("Error receiving contacts"),
                                           () => Clients.Caller.OnCompleted());

            _subscription.Disposable = subscription;
        }

        public override Task OnDisconnected()
        {
            Debug.Print("ContactProfileHub.OnDisconnected()");
            _subscription.Dispose();
            return base.OnDisconnected();
        }
    }
}