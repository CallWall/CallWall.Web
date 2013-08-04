using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using CallWall.Web.Providers;
using CallWall.Web.Providers.Google;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contacts")]
    public class ContactsHub : Hub
    {
        public void RequestContactSummaryStream()
        {
            var securityProvider = new SecurityProvider();
            var session = securityProvider.GetSession(Context, "google");

            if (session == null)
                return;
            var gContactProvider = new GoogleContactsProvider();
            var contacts = gContactProvider.GetContacts(session);

            foreach (var contact in contacts)
            {
                Clients.Caller.ReceiveContactSummary(contact);
            }
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
    public class CustomSessionSecurityTokenHandler : SessionSecurityTokenHandler
    {
        protected override void ValidateSession(SessionSecurityToken securityToken)
        {


            base.ValidateSession(securityToken);

            var ident = securityToken.ClaimsPrincipal.Identity as ClaimsIdentity;

            if (ident == null)
                throw new SecurityTokenException();

            var isa = ident.Claims.First().ValueType == ClaimTypes.Sid;
            var sessionClaim = ident.Claims.FirstOrDefault(c => c.ValueType == ClaimTypes.Sid);

            if (sessionClaim == null)
                throw new SecurityTokenExpiredException();

            //if (!NotificationHub.IsSessionValid(sessionClaim.Value))
            //{
            //    throw new SecurityTokenExpiredException();
            //}
        }
    }
}