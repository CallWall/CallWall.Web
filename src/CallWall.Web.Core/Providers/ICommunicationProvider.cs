using System;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface ICommunicationProvider
    {
        IObservable<IMessage> GetMessages(User user, string[] contactKeys);
    }
}