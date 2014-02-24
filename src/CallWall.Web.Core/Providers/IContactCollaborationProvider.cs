using System;
using System.Collections.Generic;
using CallWall.Web.Contracts;

namespace CallWall.Web.Providers
{
    public interface IContactCollaborationProvider
    {
        IObservable<IContactCollaboration> GetCollaborations(IEnumerable<ISession> session, string[] contactKeys);
    }
}