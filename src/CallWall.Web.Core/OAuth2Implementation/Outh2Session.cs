using System;
using System.Collections.Generic;

namespace CallWall.Web.OAuth2Implementation
{
    public sealed class OAuthSession : Session, ISession
    {
        public OAuthSession(string accessToken, string refreshToken, TimeSpan accessPeriod, DateTimeOffset requested, IEnumerable<string> authorizedResources)
            : base(accessToken, refreshToken, requested + accessPeriod, authorizedResources)
        {
        }

        public OAuthSession(string accessToken, string refreshToken, DateTimeOffset expires, IEnumerable<string> authorizedResources)
            :base(accessToken, refreshToken, expires, authorizedResources)
        {
        }

        public override string ToString()
        {
            return string.Format("OAuthSession {{ AccessToken : '{0}', RefreshToken : '{1}', Expires : '{2:o}', AuthorizedResources : '{3}'}}", AccessToken, RefreshToken, Expires, string.Join(";", AuthorizedResources));
        }
    }
}
