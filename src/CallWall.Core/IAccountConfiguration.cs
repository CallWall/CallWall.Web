using System;
using System.Collections.Generic;

namespace CallWall
{
    public interface IAccountConfiguration
    {
        //bool IsEnabled { get; set; }
        string Name { get; }
        Uri Image { get; }
        IEnumerable<IResourceScope> Resources { get; }
    }
}
