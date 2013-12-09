using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.OAuth2Implementation
{
    public sealed class Outh2Session : ISession
    {
        private readonly string _accessToken;
        private readonly string _refreshToken;
        private readonly DateTimeOffset _expires;
        private readonly ISet<string> _authorizedResources;
        private readonly string _provider;

        public Outh2Session(string provider, string accessToken, string refreshToken, TimeSpan accessPeriod, DateTimeOffset requested, IEnumerable<string> authorizedResources)
            : this(provider, accessToken, refreshToken, requested + accessPeriod, authorizedResources)
        {}

        public Outh2Session(string provider, string accessToken, string refreshToken, DateTimeOffset expires, IEnumerable<string> authorizedResources)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _expires = expires;
            _provider = provider;
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
            return string.Format("Session {{ Provider : '{0}', AccessToken : '{1}', RefreshToken : '{2}', Expires : '{3:o}', AuthorizedResources : '{4}'}}",Provider, AccessToken, RefreshToken, Expires, string.Join(";", AuthorizedResources));
        }
    }
}