using System;

namespace CallWall.Web.Models
{
    public interface IProviderDescription
    {
        string Name { get; }
        Uri Image { get; }
    }
}