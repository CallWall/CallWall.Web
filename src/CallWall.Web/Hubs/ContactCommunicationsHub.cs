using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.Providers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactCommunications")]
    public class ContactCommunicationsHub : Hub
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly IEnumerable<ICommunicationProvider> _communicationProviders;
        private readonly ILoginProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCommunicationsHub(IEnumerable<ICommunicationProvider> communicationProviders, ILoginProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCommunicationsHub.ctor()");
            _communicationProviders = communicationProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCommunicationsHub.ctor(communicationProviders:{0})", string.Join(",", _communicationProviders.Select(cp => cp.GetType().Name)));
        }

        public async Task Subscribe(string[] contactKeys)
        {
            Debug.Print("ContactCommunicationsHub.Subscribe(...)");
            var user = await _sessionProvider.GetUser(Context.User.UserId());
            var subscription = _communicationProviders
                                .ToObservable()
                                .SelectMany(c => c.GetMessages(user, contactKeys))
                                .Log(_logger, "GetMessages")
                                .Subscribe(msg => Clients.Caller.OnNext(msg),
                                           ex => Clients.Caller.OnError("Error receiving communication messages"),
                                           () => Clients.Caller.OnCompleted());

            _subscription.Disposable = subscription;
        }

        public override Task OnDisconnected()
        {
            Debug.Print("ContactCommunicationsHub.OnDisconnected()");
            _subscription.Dispose();
            return base.OnDisconnected();
        }
    }
}