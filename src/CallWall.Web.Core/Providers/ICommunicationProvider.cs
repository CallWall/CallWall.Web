using System;
using System.Collections.Generic;
using CallWall.Web.Contracts.Communication;

namespace CallWall.Web.Providers
{
    public interface ICommunicationProvider
    {
        IObservable<IMessage> GetMessages(IEnumerable<ISession> session, string[] contactKeys);
    }

}