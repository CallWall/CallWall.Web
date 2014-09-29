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
        private readonly ILoginProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCollaborationHub(IEnumerable<IContactCollaborationProvider> collaborationProviders, ILoginProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCollaborationHub.ctor()");
            _collaborationProviders = collaborationProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCollaborationHub.ctor(collaborationProviders:{0})", string.Join(",", _collaborationProviders.Select(cp=>cp.GetType().Name)));
        }

        public async Task Subscribe(string[] contactKeys)
        {
            Debug.Print("ContactProfileHub.Subscribe(...)");
            var user = await _sessionProvider.GetUser(Context.User.UserId());
            var sessions = user.Accounts.Select(a => a.CurrentSession).ToArray();
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