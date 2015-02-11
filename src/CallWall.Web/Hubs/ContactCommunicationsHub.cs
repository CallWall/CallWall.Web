﻿using System;
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
    [HubName("contactCommunications")]
    public class ContactCommunicationsHub : Hub
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly IEnumerable<ICommunicationProvider> _communicationProviders;
        private readonly IContactRepository _contactRepository;
        private readonly ILoginProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCommunicationsHub(IContactRepository contactRepository, IEnumerable<ICommunicationProvider> communicationProviders, ILoginProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCommunicationsHub.ctor()");
            _contactRepository = contactRepository; 
            _communicationProviders = communicationProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCommunicationsHub.ctor(communicationProviders:{0})", string.Join(",", _communicationProviders.Select(cp => cp.GetType().Name)));
        }

        public async Task Subscribe(string contactId)
        {
            try
            {
                Debug.Print("ContactCommunicationsHub.Subscribe(...)");
                var user = await _sessionProvider.GetUser(Context.User.UserId());
                var query = from contactProfile in _contactRepository.GetContactDetails(user, contactId)
                            from commProvider in _communicationProviders
                            from message in commProvider.GetMessages(user, contactProfile.ContactKeys())
                            select message;
                var subscription = query
                    .Log(_logger, "GetMessages")
                    .Subscribe(msg => Clients.Caller.OnNext(msg),
                        ex => Clients.Caller.OnError("Error receiving communication messages"),
                        () => Clients.Caller.OnCompleted());

                _subscription.Disposable = subscription;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Requesting ContactCommunicationStream in Hub.");
                Clients.Caller.OnError("Error receiving contact communication data");
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Debug.Print("ContactCommunicationsHub.OnDisconnected({0})", stopCalled);
            _subscription.Dispose();
            return base.OnDisconnected(stopCalled);
        }
    }
}