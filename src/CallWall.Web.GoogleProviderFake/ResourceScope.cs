using System;
using System.Collections.ObjectModel;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProviderFake
{
    public sealed class ResourceScope : IResourceScope
    {
        public static readonly ResourceScope Contacts;
        public static readonly ResourceScope Gmail;
        public static readonly ResourceScope Calendar;

        private static readonly ReadOnlyCollection<ResourceScope> _availableResourceScopes;

        private readonly string _name;
        private readonly Uri _image;
        private readonly string _resource;

        static ResourceScope()
        {
            Contacts = new ResourceScope("Contacts(Fake)", "Contacts_48x48.png", @"https://www.google.com/m8/feeds/");
            Gmail = new ResourceScope("Email(Fake)", "Email_48x48.png", @"https://mail.google.com/");
            Calendar = new ResourceScope("Calendar(Fake)", "Calendar_48x48.png", null);
            _availableResourceScopes = new ReadOnlyCollection<ResourceScope>(new[]
                {
                    Contacts,
                    Gmail,
                    //Calendar
                });
        }

        private ResourceScope(string name, string image, string resource)
        {
            _name = name;
            //TODO: How do I get content/resources/images from a Provider build into dir that is safe for the web to serve? -LC
            //_image = new Uri(string.Format("/Providers/GoogleFakeContent/{0}", image), UriKind.Relative);
            _image = new Uri(string.Format("/Content/Images/Google/{0}", image), UriKind.Relative);
            _resource = resource;
        }

        public static ReadOnlyCollection<ResourceScope> AvailableResourceScopes
        {
            get { return _availableResourceScopes; }
        }

        public string Name { get { return _name; } }

        public string Resource { get { return _resource; } }

        public Uri Image { get { return _image; } }

        public override string ToString()
        {
            return string.Format("Google.ResourceScope{{{0}}}", Name);
        }
    }
}