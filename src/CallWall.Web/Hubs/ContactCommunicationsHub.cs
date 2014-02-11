using System;
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
    public class ContactCommunicationsHub : ObservableHub<Message>
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        public ContactCommunicationsHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<Message> provider) : 
        private readonly ISessionProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCommunicationsHub(IEnumerable<ICommunicationProvider> communicationProviders, ISessionProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCommunicationsHub.ctor()");
            _communicationProviders = communicationProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCommunicationsHub.ctor(communicationProviders:{0})", string.Join(",", _communicationProviders.Select(cp => cp.GetType().Name)));
        }

        public void Subscribe(string[] contactKeys)
        {
            Debug.Print("ContactCommunicationsHub.Subscribe(...)");
            var sessions = _sessionProvider.GetSessions(Context.User);
            var subscription = _communicationProviders
                                .ToObservable()
                                .SelectMany(c => c.GetMessages(sessions, contactKeys))
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