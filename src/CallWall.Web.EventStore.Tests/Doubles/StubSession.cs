using System;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubSession : Session
    {
        public StubSession(params string[] authorizedResources)
            :base("StubAccessToken", "StubRefreshToken", DateTimeOffset.Now.AddHours(1), authorizedResources)
        {
        }
    }
}