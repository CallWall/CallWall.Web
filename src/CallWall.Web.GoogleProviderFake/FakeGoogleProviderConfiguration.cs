using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleProviderConfiguration : IProviderConfiguration
    {
        public static readonly IProviderConfiguration Instance = new FakeGoogleProviderConfiguration();

        private FakeGoogleProviderConfiguration()
        {}

        public string Name { get { return Constants.ProviderName; } }
        //TODO: How do I get content/resources/images from a Provider build into dir that is safe for the web to serve? -LC
        //public Uri Image { get { return new Uri("/Providers/GoogleFakeContent/GoogleIcon.svg", UriKind.Relative); } }
        public Uri Image { get { return new Uri("/Content/Google/Images/GoogleIcon.svg", UriKind.Relative); } }
        public IEnumerable<IResourceScope> Resources { get { return ResourceScope.AvailableResourceScopes; } }
    }
}