using System;

namespace CallWall.Web.Contracts
{
    public interface IProviderDescription
    {
        string Name { get; }
        Uri Image { get; }
    }
}