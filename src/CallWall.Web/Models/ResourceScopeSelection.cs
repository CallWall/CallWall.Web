using System;
using CallWall.Web.Domain;

namespace CallWall.Web.Models
{
    public class ResourceScopeSelection
    {
        public ResourceScopeSelection(IResourceScope resourceScope, bool isAuthorized)
        {
            Name = resourceScope.Name;
            Resource = resourceScope.Resource;
            Image = resourceScope.Image;
            IsAuthorized = isAuthorized;
        }
        public string Name { get; set; }
        public string Resource { get; set; }
        public Uri Image { get; set; }
        public bool IsAuthorized { get; set; }
    }
}