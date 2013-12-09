using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;

namespace CallWall.Web.OAuth2Implementation
{
    internal class AuthState
    {
        public string Account { get; set; }
        public string RedirectUri { get; set; }
        public IEnumerable<string> Scopes { get; set; }

        public static AuthState Deserialize(string state)
        {
            return JsonConvert.DeserializeObject<AuthState>(state);
        }

        public string ToUrlEncoded()
        {
            var data = JsonConvert.SerializeObject(this);
            return HttpUtility.UrlEncode(data);
        }
    }
}