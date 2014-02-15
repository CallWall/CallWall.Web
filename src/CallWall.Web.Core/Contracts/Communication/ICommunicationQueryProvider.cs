using System;
using Newtonsoft.Json.Serialization;

namespace CallWall.Web.Contracts.Communication
{
    [Obsolete("use ICommunicationProvider", error: true)]
    public interface ICommunicationQueryProvider
    {
        IObservable<IMessage> LoadMessages(IProfile activeProfile);
    }
}
