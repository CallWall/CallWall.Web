using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using CallWall.Web.Account;

namespace CallWall.Web.OAuth2Implementation
{
    public abstract class OAuth2AuthenticationBase
    {
        public abstract string RequestAuthorizationBaseUri { get; }

        public abstract string RequestTokenAccessBaseUri { get; }

        public abstract string ClientId { get; }

        public abstract string ClientSecret { get; }

        public abstract string ProviderName { get; }

        protected abstract IAccount CreateAccount();

        public Uri AuthenticationUri(string redirectUri, IList<string> scopes)
        {
            var uriBuilder = new StringBuilder();
            uriBuilder.Append(RequestAuthorizationBaseUri);
            uriBuilder.Append("?scope=");
            var scopeSsv = string.Join(" ", scopes);
            uriBuilder.Append(HttpUtility.UrlEncode(scopeSsv));

            uriBuilder.Append("&redirect_uri=");
            uriBuilder.Append(HttpUtility.UrlEncode(redirectUri));

            uriBuilder.Append("&response_type=code");

            uriBuilder.Append("&client_id=");
            uriBuilder.Append(ClientId);

            var state = new AuthState { RedirectUri = redirectUri, Scopes = scopes, Account = ProviderName };
            uriBuilder.Append("&state=");
            uriBuilder.Append(state.ToUrlEncoded());

            return new Uri(uriBuilder.ToString());
        }

        public bool CanCreateSessionFromState(string code, string state)
        {
            return IsValidOAuthState(state);
        }

        public bool IsValidOAuthState(string state)
        {
            var json = JObject.Parse(state);
            JToken account;
            if (json.TryGetValue("Account", out account))
            {
                if (account.ToString() == ProviderName)
                {
                    return true;
                }
            }
            return false;
        }

        public ISession CreateSession(string code, string state)
        {
            var authState = AuthState.Deserialize(state);
            var request = CreateTokenRequest(code, authState.RedirectUri);

            var client = new HttpClient();
            var response = client.SendAsync(request);
            var accessTokenResponse = response.Result.Content.ReadAsStringAsync();
            var json = JObject.Parse(accessTokenResponse.Result);

            DemandValidTokenResponse(json);

            var account = CreateAccount();

            return new OAuthSession((string)json["access_token"], (string)json["refresh_token"], TimeSpan.FromSeconds((int)json["expires_in"]), DateTimeOffset.Now, ProviderName, account, authState.Scopes);
        }

        public bool TryDeserialiseSession(string payload, out ISession session)
        {
            session = null;
            try
            {
                var jsonContainer = JObject.Parse(payload);
                var json = jsonContainer[ProviderName];
                var authorizedResources = json["AuthorizedResources"].ToObject<IEnumerable<string>>();

                var account = CreateAccount();

                session = new OAuthSession((string)json["AccessToken"], (string)json["RefreshToken"], (DateTimeOffset)json["Expires"], ProviderName, account, authorizedResources);
                return true;
            }
            catch (Exception)
            {
                //TODO: Log this failure as Trace/Debug
                return false;
            }
        }

        private HttpRequestMessage CreateTokenRequest(string code, string redirectUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, RequestTokenAccessBaseUri);
            var postParameters = new Dictionary<string, string>
                    {
                        {"code", code},
                        {"client_id", ClientId},
                        {"redirect_uri", redirectUri},
                        {"client_secret", ClientSecret},
                        {"grant_type", "authorization_code"}
                    };

            request.Content = new FormUrlEncodedContent(postParameters);
            return request;
        }

        protected abstract void DemandValidTokenResponse(JObject json);
    }
}
