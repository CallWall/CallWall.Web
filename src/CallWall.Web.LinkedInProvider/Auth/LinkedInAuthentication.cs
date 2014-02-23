using System.Security.Authentication;
using CallWall.Web.OAuth2Implementation;
using Newtonsoft.Json.Linq;
using CallWall.Web.Account;

namespace CallWall.Web.LinkedInProvider.Auth
{
    public class LinkedInAuthentication : OAuth2AuthenticationBase, IAccountAuthentication
    {
        public IAccountConfiguration Configuration { get { return AccountConfiguration.Instance; } }

        public override string RequestAuthorizationBaseUri { get { return "https://www.linkedin.com/uas/oauth2/authorization"; } }

        public override string RequestTokenAccessBaseUri
        {
            get { return "https://www.linkedin.com/uas/oauth2/accessToken"; }
        }

        public override string ClientId
        {
            get { return "751tu6va8d937l"; }
        }

        public override string ClientSecret
        {
            get { return "MHOnHig0JXIwCd49"; }
        }

        public override string ProviderName
        {
            get { return "LinkedIn"; }
        }

        protected override IAccount CreateAccount()
        {
            return new Account("TODO userName", "TODO displayName");
        }

        protected override void DemandValidTokenResponse(JObject json)
        {
            if (json["error"] == null)
                return;
            if (json["error_description"] != null)
                throw new AuthenticationException(string.Format("{0} : {1}", json["error"], json["error_description"]));
            throw new AuthenticationException((string)json["error"]);
        }
    }
}