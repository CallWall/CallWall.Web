using System;
using CallWall.Web.Contracts;

namespace CallWall.Web.GoogleProvider.Providers.Gmail
{
    public sealed class GmailProviderDescription : IProviderDescription
    {
        public static readonly GmailProviderDescription Instance = new GmailProviderDescription();

        private GmailProviderDescription()
        { }

        public string Name
        {
            get { return "Gmail"; }
        }

        public Uri Image
        {
            get { return new Uri("/Content/Google/Images/Email_48x48.png", UriKind.Relative); }
        }
    }
}