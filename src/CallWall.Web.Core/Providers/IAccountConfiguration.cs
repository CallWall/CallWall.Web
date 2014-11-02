using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web
{
    public interface IAccountConfiguration
    {
        string Name { get; }
        Uri Image { get; }
        IEnumerable<IResourceScope> Resources { get; }
    }
}
