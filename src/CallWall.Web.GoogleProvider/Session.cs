using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.GoogleProvider
{
    internal sealed class Session : ISession
    {
        private readonly string _accessToken;
        private readonly string _refreshToken;
        private readonly DateTimeOffset _expires;
        private readonly ISet<string> _authorizedResources;

        public Session(string accessToken, string refreshToken, TimeSpan accessPeriod, DateTimeOffset requested, IEnumerable<Uri> authorizedResources, IAccount account)
            : this(accessToken, refreshToken, requested + accessPeriod, authorizedResources, account)
        {
        }

        public Session(string accessToken, string refreshToken, DateTimeOffset expires, IEnumerable<Uri> authorizedResources, IAccount account)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _expires = expires;
            _authorizedResources = new HashSet<string>(authorizedResources);
        }

        public string Provider { get { return "Google"; } }
        public string AccessToken { get { return _accessToken; } }
        public string RefreshToken { get { return _refreshToken; } }
        public DateTimeOffset Expires { get { return _expires; } }

        public bool HasExpired()
        {
            return DateTimeOffset.Now > _expires;
        }

        public IAccount AccountDetails { get { return _account; } }

        public ISet<Uri> AuthorizedResources
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
            return string.Format("Session {{ AccessToken : '{0}', RefreshToken : '{1}', Expires : '{2:o}', AuthorizedResources : '{3}', AccountDetails : '{4}' }}", AccessToken, RefreshToken, Expires, string.Join(";", AuthorizedResources), AccountDetails);
        }
    }
}
