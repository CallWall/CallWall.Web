using System;
using System.Security;
using System.Security.Principal;
using System.Web.Security;

namespace CallWall.Web
{
    public static class PrincipalEx
    {
        public static Guid UserId(this IPrincipal principal)
        {
            if (!principal.Identity.IsAuthenticated) throw new SecurityException();
            var formsIdentitiy = (FormsIdentity)principal.Identity;
            var userId = formsIdentitiy.Ticket.UserData;

            return Guid.Parse(userId);
        }
    }
}