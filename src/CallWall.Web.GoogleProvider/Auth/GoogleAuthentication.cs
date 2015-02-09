using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Providers.Contacts;
using CallWall.Web.OAuth2Implementation;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.GoogleProvider.Auth
{
    public sealed class GoogleAuthentication : OAuth2AuthenticationBase, IAccountAuthentication
    {
        private readonly IGoogleAccountProvider _googleAccountProvider;

        public GoogleAuthentication(IGoogleAccountProvider googleAccountProvider)
        {
            _googleAccountProvider = googleAccountProvider;
        }

        public override string RequestAuthorizationBaseUri
        {
            get { return "https://accounts.google.com/o/oauth2/auth"; }
        }

        public override string RequestTokenAccessBaseUri
        {
            get { return "https://accounts.google.com/o/oauth2/token"; }
        }

        public override string ClientId
        {
            get { return "410654176090-8fk01hicm60blfbmjfrfruvpabnvat6s.apps.googleusercontent.com"; }
        }

        public override string ClientSecret
        {
            get { return "cl6V2rzrB0uit3mHDB2jAmnG"; }
        }

        public override string ProviderName
        {
            get { return Constants.ProviderName; }
        }

        public override IAccountConfiguration Configuration
        {
            get { return AccountConfiguration.Instance; }
        }

        protected override async Task<IAccount> CreateAccount(ISession session)
        {
            return await _googleAccountProvider.CreateAccount(session);
        }

        protected override void DemandValidTokenResponse(JObject json)
        {
           //no op
        }
    }
}