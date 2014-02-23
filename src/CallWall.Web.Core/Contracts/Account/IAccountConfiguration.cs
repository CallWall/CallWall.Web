using System;
using System.Collections.Generic;

namespace CallWall.Web.Account
{
    public interface IAccountConfiguration
    {
        string Name { get; }
        Uri Image { get; }
        IEnumerable<IResourceScope> Resources { get; }
    }
}
