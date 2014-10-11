using System;
using System.Collections.Generic;
using CallWall.Web.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.GoogleProviderFake
{
    sealed class FakeSession : ISession
    {
        private readonly HashSet<string> _authorizedResources;

        public FakeSession(IEnumerable<string> scopes)
        {
            _authorizedResources = new HashSet<string>(scopes);
        }
        
        public string AccessToken { get { return "FakeAccessToken"; } }
        public string RefreshToken { get { return "FakeRefreshToken"; } }
        public DateTimeOffset Expires { get { return DateTimeOffset.Now.AddHours(1); } }
        public ISet<string> AuthorizedResources { get { return _authorizedResources; } }

        public bool HasExpired()
        {
            return false;
        }
        public string Serialize()
        {
            var jObject = JObject.FromObject(this);
            var json = jObject.ToString(Formatting.None);
            return json;
        }
    }
}