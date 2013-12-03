using System;
using System.Collections.Generic;

namespace CallWall.Web.Providers
{
    public interface IAuthenticationProviderGateway
    {
        IAccountAuthentication GetAuthenticationProvider(string account);
        IEnumerable<IAccountConfiguration> GetAccountConfigurations();
        Uri AuthenticationUri(string account, string callBackUri, string[] resource);
    }
}