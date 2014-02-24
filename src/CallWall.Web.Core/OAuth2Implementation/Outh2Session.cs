using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CallWall.Web.Account;

namespace CallWall.Web.OAuth2Implementation
{
    public sealed class OAuthSession : ISession
    {
        private readonly string _accessToken;
        private readonly string _refreshToken;
        private readonly DateTimeOffset _expires;
        private readonly ISet<string> _authorizedResources;
        private readonly string _provider;
        private readonly IAccount _accountDetails;

        public OAuthSession(string accessToken, string refreshToken, TimeSpan accessPeriod, DateTimeOffset requested, string provider, IAccount accountDetails, IEnumerable<string> authorizedResources)
            : this(accessToken, refreshToken, requested + accessPeriod, provider, accountDetails, authorizedResources)
        {
        }

        public OAuthSession(string accessToken, string refreshToken, DateTimeOffset expires, string provider, IAccount accountDetails, IEnumerable<string> authorizedResources)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _expires = expires;
            _provider = provider;
            _accountDetails = accountDetails;
            _authorizedResources = new HashSet<string>(authorizedResources);
        }

        public string Provider { get { return _provider; } }
        public string AccessToken { get { return _accessToken; } }
        public string RefreshToken { get { return _refreshToken; } }
        public DateTimeOffset Expires { get { return _expires; } }

        public bool HasExpired()
        {
            return DateTimeOffset.Now > _expires;
        }

        public IAccount AccountDetails { get { return _accountDetails; } }

        public ISet<string> AuthorizedResources
        {
            get { return _authorizedResources; }
        }

        public string Serialize()
        {
            var jObject = JObject.FromObject(this);
            var json = jObject.ToString(Formatting.None);
            return json;
        }

        public override string ToString()
        {
            return string.Format("OAuthSession {{ AccessToken : '{0}', RefreshToken : '{1}', Expires : '{2:o}', AuthorizedResources : '{3}', AccountDetails : '{4}' }}", AccessToken, RefreshToken, Expires, string.Join(";", AuthorizedResources), AccountDetails);
        }
    }
}
