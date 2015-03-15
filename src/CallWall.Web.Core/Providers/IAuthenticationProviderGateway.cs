using System;
using System.Collections.Generic;

namespace CallWall.Web.Providers
{
    public interface IAuthenticationProviderGateway
    {
        IEnumerable<IProviderConfiguration> GetProviderConfigurations();
        Uri AuthenticationUri(string providerName, string callBackUri, string[] resource);
    }
}