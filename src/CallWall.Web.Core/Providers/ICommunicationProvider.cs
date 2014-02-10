using System;
using System.Collections.Generic;

namespace CallWall.Web.Providers
{
    public interface ICommunicationProvider
    {
        IObservable<IMessage> GetMessages(IEnumerable<ISession> session, string[] contactKeys);
    }
}