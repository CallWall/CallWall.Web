using System;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface ICommunicationProvider
    {
        //TODO: I think we wnat to be able to expose messages and provider failures
        //   potentially upgrade this to IObs<Either<IMessage, Failure>>? -LC
        IObservable<IMessage> GetMessages(User user, string[] contactKeys);
    }
}