using System;
using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public class AuthenticationProviderGateway : IAuthenticationProviderGateway
    {
        private readonly Dictionary<string, IAccountAuthentication> _authenticationProvidersMap;

        public AuthenticationProviderGateway(IEnumerable<IAccountAuthentication> authenticationProviders)
        {
            _authenticationProvidersMap = authenticationProviders.ToDictionary(ap => ap.Configuration.Name);
        }

        public IEnumerable<IProviderConfiguration> GetProviderConfigurations()
        {
            return _authenticationProvidersMap.Values.Select(ap => ap.Configuration);
        }

        public Uri AuthenticationUri(string providerName, string callBackUri, string[] resources)
        {
            var ap = GetAuthenticationProvider(providerName);
            return ap.AuthenticationUri(callBackUri, resources);
        }

        private IAccountAuthentication GetAuthenticationProvider(string providerName)
        {
            return _authenticationProvidersMap[providerName];
        }
    }
}