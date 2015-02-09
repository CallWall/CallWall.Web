using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.Domain;
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
        private readonly IContactRepository _contactRepository;
        private readonly ILoginProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCollaborationHub(IContactRepository contactRepository, IEnumerable<IContactCollaborationProvider> collaborationProviders, ILoginProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCollaborationHub.ctor()");
            _contactRepository = contactRepository; 
            _collaborationProviders = collaborationProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCollaborationHub.ctor(collaborationProviders:{0})", string.Join(",", _collaborationProviders.Select(cp=>cp.GetType().Name)));
        }

        public async Task Subscribe(string contactId)
        {
            try
            {
                Debug.Print("ContactProfileHub.Subscribe(...)");
                var user = await _sessionProvider.GetUser(Context.User.UserId());

                var query = from contactProfile in _contactRepository.GetContactDetails(user, contactId)
                            from collaborationProvider in _collaborationProviders
                            from collaboration in collaborationProvider.GetCollaborations(user, contactProfile.ContactKeys())
                            select collaboration;

                var subscription = query.Log(_logger, "GetCollaborations")
                    .Subscribe(collaboration => Clients.Caller.OnNext(collaboration),
                        ex => Clients.Caller.OnError("Error receiving Collaboration data"),
                        () => Clients.Caller.OnCompleted());

                _subscription.Disposable = subscription;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Requesting ContactCollaborationStream in Hub.");
                Clients.Caller.OnError("Error receiving Collaboration data");
            }
        }

        public override Task OnDisconnected()
        {
            Debug.Print("ContactCollaborationHub.OnDisconnected()");
            _subscription.Dispose();
            return base.OnDisconnected();
        }
    }
}