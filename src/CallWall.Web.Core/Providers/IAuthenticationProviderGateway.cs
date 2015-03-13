using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface IAuthenticationProviderGateway
    {
        IAccountAuthentication GetAuthenticationProvider(string account);
        IEnumerable<IProviderConfiguration> GetProviderConfigurations();
        Uri AuthenticationUri(string account, string callBackUri, string[] resource);
    }
}