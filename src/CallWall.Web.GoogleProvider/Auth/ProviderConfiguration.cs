using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Auth
{
    internal sealed class ProviderConfiguration : IProviderConfiguration
    {
        public static readonly IProviderConfiguration Instance = new ProviderConfiguration();

        private ProviderConfiguration()
        {}

        public string Name { get { return Constants.ProviderName; } }
        public Uri Image { get { return new Uri("/Content/Google/Images/GoogleIcon.svg", UriKind.Relative); } }
        public IEnumerable<IResourceScope> Resources { get { return ResourceScope.AvailableResourceScopes; } }
    }
}