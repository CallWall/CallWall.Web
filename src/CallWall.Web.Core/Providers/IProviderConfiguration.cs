using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web
{
    //TODO: COnsider a rename from *Configuration as this isn't really config. Maybe metadata, features, description... -LC
    public interface IProviderConfiguration
    {
        string Name { get; }
        Uri Image { get; }
        IEnumerable<IResourceScope> Resources { get; }
    }
}
