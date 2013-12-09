using System;
using System.Collections.Generic;
using System.Linq;

namespace CallWall.Web.Providers
{
    public class AuthenticationProviderGateway : IAuthenticationProviderGateway
    {
        private readonly IEnumerable<IAccountAuthentication> _authenticationProviders;

        public AuthenticationProviderGateway(IEnumerable<IAccountAuthentication> authenticationProviders)
        {
            _authenticationProviders = authenticationProviders;
        }

        //NOTE : Can be made private - only usage is here
        public IAccountAuthentication GetAuthenticationProvider(string account)
        {
            return _authenticationProviders.Single(ap => string.Equals(ap.Configuration.Name, account, StringComparison.Ordinal));
        }

        public IEnumerable<IAccountConfiguration> GetAccountConfigurations()
        {
            return _authenticationProviders.Select(ap => ap.Configuration);
        }

        public Uri AuthenticationUri(string account, string callBackUri, string[] resources)
        {
            var ap = GetAuthenticationProvider(account);
            return ap.AuthenticationUri(callBackUri, resources);
        }
    }
}