using System;
using System.Collections.ObjectModel;

namespace CallWall.Web.LinkedInProvider
{
    public sealed class ResourceScope : IResourceScope
    {
        public static readonly ResourceScope Connections;
       
        private static readonly ReadOnlyCollection<ResourceScope> _availableResourceScopes;

        private readonly string _name;
        private readonly Uri _image;
        private readonly string _resource;

        static ResourceScope()
        {
            Connections = new ResourceScope("Connections", "Contacts_48x48.png", "r_network");
            _availableResourceScopes = new ReadOnlyCollection<ResourceScope>(new[]
                {
                    Connections
                });
        }

        private ResourceScope(string name, string image, string resource)
        {
            _name = name;
            _image = new Uri(string.Format("/Content/LinkedIn/Images/{0}", image), UriKind.Relative);
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
            return string.Format("LinkedIn.ResourceScope{{{0}}}", Name);
        }
    }
}