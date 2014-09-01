using System;
using System.Linq;

namespace CallWall.Web.EventStore.Accounts
{
    public class SessionRecord : ISession
    {
        public SessionRecord()
        {
        }

        public SessionRecord(ISession source)
        {
            AccessToken = source.AccessToken;
            RefreshToken = source.RefreshToken;
            Expires = source.Expires;
            AuthorizedResources = source.AuthorizedResources.ToArray();
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset Expires { get; set; }
        public string[] AuthorizedResources { get; set; }

        string ISession.AccessToken { get { return AccessToken; } }

        string ISession.RefreshToken { get { return RefreshToken; } }

        DateTimeOffset ISession.Expires { get { return Expires; } }

        System.Collections.Generic.ISet<string> ISession.AuthorizedResources { get { return AuthorizedResources.ToSet(); } }

        bool ISession.HasExpired()
        {
            return DateTimeOffset.Now > Expires;
        }
    }
}