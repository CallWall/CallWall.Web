using System;
using System.Reactive.Disposables;
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
            var subscription = _contactsProvider.GetContacts(session)
                                                .Subscribe(contact => Clients.Caller.ReceiveContactSummary(contact));
            _contactsSummarySubsription.Disposable = subscription;
        }
        //public void RequestContactSummaryStream()
        //{
        //    if (this.Context.User.Identity.IsAuthenticated)
        //    {
        //        Clients.Caller.ReceiveContactSummary(new ContactSummary("Fake", null, new[] { "Fake", "Test" }));
        //    }
        //}

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            _contactsSummarySubsription.Disposable = Disposable.Empty;
            return base.OnDisconnected();
        }

        protected override void Dispose(bool disposing)
        {
            _contactsSummarySubsription.Dispose();
            base.Dispose(disposing);
        }
    }
}