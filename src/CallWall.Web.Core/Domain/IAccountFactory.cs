using System.Collections.Generic;

namespace CallWall.Web.Domain
{
    public interface IAccountFactory
    {
        IAccount Create(string accountId, string provider, string displayName, ISession session, IEnumerable<ContactHandle> handles);
    }
}