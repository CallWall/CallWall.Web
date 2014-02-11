using System;

namespace CallWall.Web.Contracts.Communication
{
    public interface ICommunicationQueryProvider
    {
        IObservable<IMessage> LoadMessages(IProfile activeProfile);
    }
}
