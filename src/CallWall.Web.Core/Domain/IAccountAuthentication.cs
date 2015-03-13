using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CallWall.Web.Domain
{
    public interface IAccountAuthentication
    {
        IProviderConfiguration Configuration { get; }

        Uri AuthenticationUri(string redirectUri, IList<string> scopes);

        bool CanCreateAccountFromState(string code, string state);

        Task<IAccount> CreateAccountFromOAuthCallback(string code, string state);
    }
}