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
        private readonly SerialDisposable _contactsSummarySubsription = new SerialDisposable();

        public ContactsHub()
        {
            
        }

        //public void RequestContactSummaryStream()
        //{
        //    var securityProvider = new SecurityProvider();
        //    var session = securityProvider.GetSession(Context, "google");

        //    if (session == null)
        //        return;
        //    var gContactProvider = new GoogleContactsProvider();
        //    var subscription = gContactProvider.GetContacts(session)
        //                                       .Subscribe(contact => Clients.Caller.ReceiveContactSummary(contact));
        //    _contactsSummarySubsription.Disposable = subscription;
        //}
        public void RequestContactSummaryStream()
        {
            if (this.Context.User.Identity.IsAuthenticated)
            {
                Clients.Caller.ReceiveContactSummary(new ContactSummary("Fake", null, new[] {"Fake", "Test"}));
            }
        }

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

    //public class SessionSecurityTokenHandler : SecurityTokenHandler
    //{
    //    public override string[] GetTokenTypeIdentifiers()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override Type TokenType
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    public override System.Collections.ObjectModel.ReadOnlyCollection<System.Security.Claims.ClaimsIdentity> ValidateToken(SecurityToken token)
    //    {
    //        return base.ValidateToken(token);
    //    }
    //}
    //public class CustomSessionSecurityTokenHandler : SessionSecurityTokenHandler
    //{
    //    protected override void ValidateSession(SessionSecurityToken securityToken)
    //    {


    //        base.ValidateSession(securityToken);

    //        var ident = securityToken.ClaimsPrincipal.Identity as ClaimsIdentity;

    //        if (ident == null)
    //            throw new SecurityTokenException();

    //        var isa = ident.Claims.First().ValueType == ClaimTypes.Sid;
    //        var sessionClaim = ident.Claims.FirstOrDefault(c => c.ValueType == ClaimTypes.Sid);

    //        if (sessionClaim == null)
    //            throw new SecurityTokenExpiredException();

    //        //if (!NotificationHub.IsSessionValid(sessionClaim.Value))
    //        //{
    //        //    throw new SecurityTokenExpiredException();
    //        //}
    //    }
    //}
}