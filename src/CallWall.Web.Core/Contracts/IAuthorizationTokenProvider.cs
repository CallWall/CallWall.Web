using System.Collections.Generic;

namespace CallWall.Web.Contracts
{
    public interface IAuthorizationTokenProvider
    {
        IEnumerable<string> RequestAccessToken(IResourceScope scope);
    }
}