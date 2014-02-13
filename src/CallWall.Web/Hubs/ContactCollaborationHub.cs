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
    [HubName("contactCollaborations")]
    public class ContactCollaborationHub : Hub
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly IEnumerable<IContactCollaborationProvider> _collaborationProviders;
        private readonly ISessionProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCollaborationHub(IEnumerable<IContactCollaborationProvider> collaborationProviders, ISessionProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCollaborationHub.ctor()");
            _collaborationProviders = collaborationProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCollaborationHub.ctor(collaborationProviders:{0})", string.Join(",", _collaborationProviders.Select(cp=>cp.GetType().Name)));
        }

        public void Subscribe(string[] contactKeys)
        {
            Debug.Print("ContactProfileHub.Subscribe(...)");
            var sessions = _sessionProvider.GetSessions(Context.User);
            var subscription = _collaborationProviders
                                .ToObservable()
                                .SelectMany(c => c.GetCollaborations(sessions, contactKeys))
                                .Log(_logger, "GetCollaborations")
                                .Subscribe(contact => Clients.Caller.OnNext(contact),
                                           ex => Clients.Caller.OnError("Error receiving Collaboration data"),
                                           () => Clients.Caller.OnCompleted());

            _subscription.Disposable = subscription;
        }

        public override Task OnDisconnected()
        {
            Debug.Print("ContactCollaborationHub.OnDisconnected()");
            _subscription.Dispose();
            return base.OnDisconnected();
        }
    }
}