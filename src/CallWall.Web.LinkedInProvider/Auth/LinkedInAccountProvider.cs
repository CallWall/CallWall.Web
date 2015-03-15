using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Xml.Linq;
using CallWall.Web.Domain;
using CallWall.Web.Http;

namespace CallWall.Web.LinkedInProvider.Auth
{
    public sealed class LinkedInAccountProvider : ILinkedInAccountProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly IAccountFactory _accountFactory;

        public LinkedInAccountProvider(IHttpClient httpClient, IAccountFactory accountFactory)
        {
            _httpClient = httpClient;
            _accountFactory = accountFactory;
        }

        public async Task<IAccount> CreateAccount(ISession session)
        {
            var request = CreateRequestParams(session.AccessToken);
            var response = await _httpClient.GetResponse(request).ToTask();
            return TranslateToAccount(response, session);
        }

        private static HttpRequestParameters CreateRequestParams(string accessToken)
        {
            var param = new HttpRequestParameters(@"https://api.linkedin.com/v1/people/~");
            param.QueryStringParameters.Add("oauth2_access_token", accessToken);
            return param;
        }

        private IAccount TranslateToAccount(string response, ISession session)
        {
            /*
<?xml version="1.0" encoding="UTF-8"?>
<person>
<id>AYG5qtgCVu</id>
<first-name>Lee</first-name>
<last-name>Campbell</last-name>
<headline>Consultant with Adaptive</headline>
<site-standard-profile-request>
<url>https://www.linkedin.com/profile/view?id=20154357&authType=name&authToken=dcRu&trk=api*a3227641*s3301901*</url>
</site-standard-profile-request>
</person>
             */

            var xDoc = XDocument.Parse(response);
            if (xDoc.Root == null)
                return null;
            var root = xDoc.Root;

            var idElement = root.Element(XName.Get("id"));
            if (idElement == null)
                return null;

            var xFirstName = root.Element(XName.Get("first-name"));
            if (xFirstName == null)
                return null;

            var xLastName = root.Element(XName.Get("last-name"));
            if (xLastName == null)
                return null;

            var id = idElement.Value;
            var name = string.Format("{0} {1}", xFirstName.Value, xLastName.Value);
            var contactHandles = new ContactHandle[0];

            return _accountFactory.Create(id, ProviderConfiguration.Instance.Name, name, session, contactHandles);
        }
    }
}