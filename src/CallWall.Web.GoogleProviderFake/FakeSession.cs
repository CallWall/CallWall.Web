using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.GoogleProviderFake
{
    class FakeSession : ISession
    {
        private readonly HashSet<Uri> _authorizedResources;

        public FakeSession(IEnumerable<string> scopes)
        {
            var authorizedScopes = scopes.Select(s => new Uri(s)).ToArray();
            _authorizedResources = new HashSet<Uri>(authorizedScopes);
        }

        public string Provider { get { return "GoogleFake"; } }
        public string AccessToken { get { return "FakeAccessToken"; } }
        public string RefreshToken { get { return "FakeRefreshToken"; } }
        public DateTimeOffset Expires { get { return DateTimeOffset.Now.AddHours(1); } }
        public ISet<Uri> AuthorizedResources { get { return _authorizedResources; } }
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