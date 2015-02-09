using System;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface IContactCollaborationProvider
    {
        IObservable<IContactCollaboration> GetCollaborations(User user, string[] contactKeys);
    }
}