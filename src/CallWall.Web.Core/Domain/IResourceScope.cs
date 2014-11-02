using System;

namespace CallWall.Web.Domain
{
    public interface IResourceScope
    {
        string Name { get; }
        string Resource { get; }
        Uri Image { get; }
    }
}