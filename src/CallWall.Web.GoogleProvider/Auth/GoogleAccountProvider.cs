using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.Http;

namespace CallWall.Web.GoogleProvider.Auth
{
    class GoogleAccountProvider : IGoogleAccountProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly IAccountTranslator _translator;

        public GoogleAccountProvider(IHttpClient httpClient, IAccountTranslator translator)
        {
            _httpClient = httpClient;
            _translator = translator;
        }

        public async Task<IAccount> CreateAccount(ISession session)
        {
            var request = CreateRequestParams(session.AccessToken);
            var response = await _httpClient.GetResponse(request).ToTask();
            return _translator.TranslateToAccount(response, session);
        }

        public static HttpRequestParameters CreateRequestParams(string accessToken)
        {
            var param = new HttpRequestParameters(@"https://www.google.com/m8/feeds/contacts/default/full");
            //TODO:Validate that I should be passing this in the query string. Surly I want this encoded in the POST stream -LC
            param.QueryStringParameters.Add("access_token", accessToken);
            param.QueryStringParameters.Add("max-results", "0");

            param.Headers.Add("GData-Version", "3.0");
            return param;
        }
    }
}