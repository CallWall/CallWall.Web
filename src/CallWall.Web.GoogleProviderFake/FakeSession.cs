using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.GoogleProviderFake
{
    sealed class FakeSession : ISession
    {
        private readonly IAccount _accountDetails;
        private readonly HashSet<string> _authorizedResources;

        public FakeSession(IAccount account, IEnumerable<string> scopes)
        {
            _accountDetails = account;
            _authorizedResources = new HashSet<string>(scopes);
        }

        public string Provider { get { return "GoogleFake"; } }
        public string AccessToken { get { return "FakeAccessToken"; } }
        public string RefreshToken { get { return "FakeRefreshToken"; } }
        public DateTimeOffset Expires { get { return DateTimeOffset.Now.AddHours(1); } }
        public ISet<string> AuthorizedResources { get { return _authorizedResources; } }
        public IAccount AccountDetails { get { return _accountDetails; } }

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

    sealed class FakeAccount : IAccount
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }
}