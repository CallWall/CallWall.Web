using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using CallWall.Web.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleAuthentication : IAccountAuthentication
    {
        public IAccountConfiguration Configuration { get { return FakeGoogleAccountConfiguration.Instance; } }

        public Uri AuthenticationUri(string redirectUri, IList<string> scopes)
        {
            var uriBuilder = new StringBuilder();
            uriBuilder.Append(redirectUri);

            uriBuilder.Append("?code=FakeCode&");

            var state = new AuthState { Scopes = scopes };
            uriBuilder.Append("&state=");
            uriBuilder.Append(state.ToUrlEncoded());

            return new Uri(uriBuilder.ToString());
        }

        public bool CanCreateAccountFromState(string code, string state)
        {
            return AuthState.IsValidOAuthState(state);
        }

        public IAccount CreateAccountFromOAuthCallback(string code, string state)
        {
            var authState = AuthState.Deserialize(state);
            var acc = CreateFakeAccount();
            acc.CurrentSession = new FakeSession(authState.Scopes);
            return acc;
        }

        private static FakeAccount CreateFakeAccount()
        {
            return new FakeAccount { DisplayName = Environment.UserName, AccountId = Environment.UserDomainName };
        }

        private class AuthState
        {
            private const string _account = "GoogleFake";

            public static bool IsValidOAuthState(string state)
            {
                var json = JObject.Parse(state);

                JToken account;
                if (json.TryGetValue("Account", out account))
                {
                    if (account.ToString() == _account)
                    {
                        return true;
                    }
                }
                return false;
            }
            public static AuthState Deserialize(string state)
            {
                return JsonConvert.DeserializeObject<AuthState>(state);
            }

            public string Account { get { return _account; } }

            public IEnumerable<string> Scopes { get; set; }

            public string ToUrlEncoded()
            {
                var data = JsonConvert.SerializeObject(this);
                return HttpUtility.UrlEncode(data);
            }
        }
    }


}