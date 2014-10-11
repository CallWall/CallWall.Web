using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.Contracts
{
    [Obsolete("Get this from the User object instead")]
    public interface IAuthorizationTokenProvider
    {
        IEnumerable<string> RequestAccessToken(IResourceScope scope);
    }
}