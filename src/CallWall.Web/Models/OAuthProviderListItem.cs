using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.Models
{
    public class OAuthProviderListItem
    {
        public OAuthProviderListItem(IProviderConfiguration providerConfiguration)
        {
            Name = providerConfiguration.Name;
            Image = providerConfiguration.Image;
            Resources = providerConfiguration.Resources;
        }

        public string Name { get; private set; }
        public Uri Image { get; private set; }
        public IEnumerable<IResourceScope> Resources { get; private set; }
    }
}