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
        private readonly ISecurityProvider _securityProvider;
        private readonly SerialDisposable _contactsSummarySubsription = new SerialDisposable();
        private ILogger _logger;

        public ContactsHub(IContactsProvider contactsProvider, ISecurityProvider securityProvider, ILoggerFactory loggerFactory)
        {
            _contactsProvider = contactsProvider;
            _securityProvider = securityProvider;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void RequestContactSummaryStream()
        {
            var session = _securityProvider.GetSession(Context.User);
            var subscription = _contactsProvider.GetContactsFeed(session)
                            .Log(_logger, "GetContactsFeed_1000ms")
                            .Do(feed=>Clients.Caller.ReceivedExpectedCount(feed.TotalResults))
                            .SelectMany(feed=>feed.Values)
                            .Subscribe(contact => Clients.Caller.ReceiveContactSummary(contact),
                                       ex =>
                                       {
                                            //TODO: _logger.Error(ex);
                                            Clients.Caller.ReceiveError("Error receiving contacts");
                                       }, 
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