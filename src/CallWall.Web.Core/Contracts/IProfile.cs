using System.Collections.Generic;

namespace CallWall.Web.Contracts
{
    public interface IProfile
    {
        IList<IPersonalIdentifier> Identifiers { get; }
    }
}