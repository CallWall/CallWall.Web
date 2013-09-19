using System;
using System.Reactive.Disposables;
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

        public ContactsHub(IContactsProvider contactsProvider, ISecurityProvider securityProvider)
        {
            _contactsProvider = contactsProvider;
            _securityProvider = securityProvider;
        }

        public void RequestContactSummaryStream()
        {
            var session = _securityProvider.GetSession(Context.User);
            //TODO: Return a header object that specifies the expected row count for a progress bar (will this work with composite streams?) -LC
            var subscription = _contactsProvider.GetContacts(session)
                                                .Subscribe(
                                                    contact => Clients.Caller.ReceiveContactSummary(contact),
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