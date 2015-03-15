using System.Security.Authentication;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.OAuth2Implementation;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.LinkedInProvider.Auth
{
    //https://developer.linkedin.com/documents/authentication
    public class LinkedInAuthentication : OAuth2AuthenticationBase, IAccountAuthentication
    {
        private readonly ILinkedInAccountProvider _accountProvider;

        public LinkedInAuthentication(ILinkedInAccountProvider accountProvider)
        {
            _accountProvider = accountProvider;
        }

        public override IProviderConfiguration Configuration { get { return ProviderConfiguration.Instance; } }

        public override string RequestAuthorizationBaseUri { get { return "https://www.linkedin.com/uas/oauth2/authorization"; } }

        public override string RequestTokenAccessBaseUri
        {
            get { return "https://www.linkedin.com/uas/oauth2/accessToken"; }
        }

        //LinkedIn API Key
        public override string ClientId
        {
            get { return "tawx8a0kimsi"; }
        }

        public override string ClientSecret
        {
            get { return "37B0QPk8ptKbzKsE"; }
        }

        public override string ProviderName
        {
            get { return "LinkedIn"; }
        }
        
        protected override void DemandValidTokenResponse(JObject json)
        {
            if (json["error"] == null)
                return;
            if (json["error_description"] != null)
                throw new AuthenticationException(string.Format("{0} : {1}", json["error"], json["error_description"]));
            throw new AuthenticationException((string)json["error"]);
        }

        protected override async Task<IAccount> CreateAccount(ISession session)
        {
            ////HACK: This should obviously go to LinkedIn and fetch the details. -LC
            //await Task.Delay(10);

            //return _accountFactory.Create("lee.ryan.campbell@gmail.com", ProviderName, "Lee HACK", session, Enumerable.Empty<ContactHandle>());

            return await _accountProvider.CreateAccount(session);
        }
    }
}