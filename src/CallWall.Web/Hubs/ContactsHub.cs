using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.Providers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contacts")]
    public class ContactsHub : Hub
    {
        private readonly IContactsProvider _contactsProvider;
        private readonly ISessionProvider _sessionProvider;
        private readonly ILogger _logger; 
        private readonly SerialDisposable _contactsSummarySubsription = new SerialDisposable();

        public ContactsHub(IContactsProvider contactsProvider, ISessionProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            _contactsProvider = contactsProvider;
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void RequestContactSummaryStream()
        {
            var session = _sessionProvider.GetSession(Context.User);
            var subscription = _contactsProvider.GetContactsFeed(session)
                            .Do(feed=>Clients.Caller.ReceivedExpectedCount(feed.TotalResults))
                            .SelectMany(feed=>feed.Values)
                            .Log(_logger, "GetContactsFeed")
                            .Subscribe(contact => Clients.Caller.ReceiveContactSummary(contact),
                                       ex => Clients.Caller.ReceiveError("Error receiving contacts"), 
                                       ()=>Clients.Caller.ReceiveComplete());
            
            _contactsSummarySubsription.Disposable = subscription;
        }
        
        public override Task OnDisconnected()
        {
            _contactsSummarySubsription.Dispose();
            return base.OnDisconnected();
        }
    }
}