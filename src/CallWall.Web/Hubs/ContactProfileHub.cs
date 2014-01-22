using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

        public void RequestContactProfile()
        {
            throw new NotImplementedException();

            //var sessions = _sessionProvider.GetSessions(Context.User);
            //var subscription = _contactsProviders
            //                    .ToObservable()
            //                    .SelectMany(c => c.GetContactDetails(sessions))
            //                    .Log(_logger, "RequestContactProfile")
            //                    .Subscribe(contact => Clients.Caller.ReceiveContactProfile(contact),
            //                               ex => Clients.Caller.ReceiveError("Error receiving contact profile"),
            //                               () => Clients.Caller.ReceiveComplete());

            //_contactProfileSubscription.Disposable = subscription;
        }

        public override Task OnDisconnected()
        {
            _contactProfileSubscription.Dispose();
            return base.OnDisconnected();
        }
    }
}