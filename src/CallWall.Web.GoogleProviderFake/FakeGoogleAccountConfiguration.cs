using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleAccountConfiguration : IAccountConfiguration
    {
        public static readonly IAccountConfiguration Instance = new FakeGoogleAccountConfiguration();

        private FakeGoogleAccountConfiguration()
        {}

        public string Name { get { return "Google (Fake)"; } }
        //TODO: How do I get content/resources/images from a Provider build into dir that is safe for the web to serve? -LC
        //public Uri Image { get { return new Uri("/Providers/GoogleFakeContent/GoogleIcon.svg", UriKind.Relative); } }
        public Uri Image { get { return new Uri("/Content/Images/Google/GoogleIcon.svg", UriKind.Relative); } }
        public IEnumerable<IResourceScope> Resources { get { return ResourceScope.AvailableResourceScopes; } }
    }
}