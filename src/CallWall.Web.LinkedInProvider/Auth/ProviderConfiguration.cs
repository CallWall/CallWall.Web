using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.LinkedInProvider.Auth
{
    public sealed class ProviderConfiguration : IProviderConfiguration
    {
        public static readonly IProviderConfiguration Instance = new ProviderConfiguration();

        private ProviderConfiguration()
        {}

        public string Name { get { return "LinkedIn"; } }
        public Uri Image { get { return new Uri("/Content/LinkedIn/Images/LinkedInLogo.png", UriKind.Relative); } }
        public IEnumerable<IResourceScope> Resources { get { return ResourceScope.AvailableResourceScopes; } }
    }
}