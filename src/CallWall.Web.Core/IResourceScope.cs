using System;

namespace CallWall.Web
{
    public interface IResourceScope
    {
        string Name { get; }
        Uri Resource { get; }
        Uri Image { get; }
    }
}