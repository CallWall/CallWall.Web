using System;

namespace CallWall.Web
{
    public interface IResourceScope
    {
        string Name { get; }
        string Resource { get; }
        Uri Image { get; }
    }
}