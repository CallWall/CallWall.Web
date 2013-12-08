using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Web;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.LinkedInProvider
{
    public class LinkedInAuthentication : IAccountAuthentication
    {
        private const string RequestTokenAccessBaseUri = "https://www.linkedin.com/uas/oauth2/accessToken";
        private const string ClientId = "751tu6va8d937l";//APIKEY - extract this (call wall test under rhys account)
        private const string ClientSecret = "MHOnHig0JXIwCd49";//secret - extract and hide this

        public IAccountConfiguration Configuration { get { return AccountConfiguration.Instance; } }

        public Uri AuthenticationUri(string redirectUri, IList<string> scopes)
        {
            var uriBuilder = new StringBuilder();
            uriBuilder.Append("https://www.linkedin.com/uas/oauth2/authorization");
            uriBuilder.Append("?scope=");
            var scopeSsv = string.Join(" ", scopes);
            uriBuilder.Append(HttpUtility.UrlEncode(scopeSsv));

            uriBuilder.Append("&redirect_uri=");
            uriBuilder.Append(HttpUtility.UrlEncode(redirectUri));

            uriBuilder.Append("&response_type=code");

            uriBuilder.Append("&client_id=");
            uriBuilder.Append(ClientId);

            var state = new AuthState { RedirectUri = redirectUri, Scopes = scopes };
            uriBuilder.Append("&state=");
            uriBuilder.Append(state.ToUrlEncoded());

            return new Uri(uriBuilder.ToString());
        }

        public bool CanCreateSessionFromState(string code, string state)
        {
            return AuthState.IsValidOAuthState(state);
        }

        public ISession CreateSession(string code, string state)
        {
            var authState = AuthState.Deserialize(state);
            var request = CreateTokenRequest(code, authState.RedirectUri);

            var client = new HttpClient();
            var response = client.SendAsync(request);
            var accessTokenResponse = response.Result.Content.ReadAsStringAsync();
            var json = JObject.Parse(accessTokenResponse.Result);

            if (json["error"] != null)
            {
                if (json["error_description"] != null)
                    throw new AuthenticationException(string.Format("{0} : {1}", json["error"], json["error_description"]));
                throw new AuthenticationException((string)json["error"]);
            }

            return new Session(
                (string)json["access_token"],
                (string)json["refresh_token"],
                TimeSpan.FromSeconds((int)json["expires_in"]),
                DateTimeOffset.Now,
                authState.Scopes);
        }

        public bool TryDeserialiseSession(string payload, out ISession session)
        {
            session = null;
            try
            {
                var json = JObject.Parse(payload);

                var authorizedResources = json["AuthorizedResources"].ToObject<IEnumerable<string>>();

                session = new Session(
                    (string)json["AccessToken"],
                    (string)json["RefreshToken"],
                    (DateTimeOffset)json["Expires"],
                    authorizedResources);
                return true;
            }
            catch (Exception)
            {
                //TODO: Log this failure as Trace/Debug
                return false;
            }
        }

        private static HttpRequestMessage CreateTokenRequest(string code, string redirectUri)
        {
            var uriBuilder = new StringBuilder();
            uriBuilder.Append(RequestTokenAccessBaseUri);
            uriBuilder.Append("?grant_type=authorization_code");

            uriBuilder.Append("&code=");
            uriBuilder.Append(code);

            uriBuilder.Append("&redirect_uri=");
            uriBuilder.Append(HttpUtility.UrlEncode(redirectUri));

            uriBuilder.Append("&client_id=");
            uriBuilder.Append(ClientId);
           
            uriBuilder.Append("&client_secret=");
            uriBuilder.Append(ClientSecret);
            
            var url = new Uri(uriBuilder.ToString());

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            return request;
        }

        private class AuthState
        {
            private const string _account = "LinkedIn";

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

            [UsedImplicitly]//used for serialisation
            public string Account
            {
                get { return _account; }
            }

            public string RedirectUri { get; set; }

            public IEnumerable<string> Scopes { get; set; }

            public string ToUrlEncoded()
            {
                var data = JsonConvert.SerializeObject(this);
                return HttpUtility.UrlEncode(data);
            }
        }
    }
}