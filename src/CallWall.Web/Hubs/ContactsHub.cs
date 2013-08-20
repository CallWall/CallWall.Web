using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Reactive.Disposables;
using System.Security.Claims;
using CallWall.Web.Models;
using CallWall.Web.Providers;
using CallWall.Web.Providers.Google;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contacts")]
    public class ContactsHub : Hub
    {
        private readonly IContactsProvider _contactsProvider;
        private readonly SerialDisposable _contactsSummarySubsription = new SerialDisposable();

        public ContactsHub(IContactsProvider contactsProvider)
        {
            _contactsProvider = contactsProvider;
        }

        public void RequestContactSummaryStream()
        {
            var session = Context.User.ToSession();
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