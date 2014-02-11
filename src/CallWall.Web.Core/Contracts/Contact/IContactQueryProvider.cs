using System;

namespace CallWall.Web.Contracts.Contact
{
    public interface IContactQueryProvider
    {
        IObservable<IContactProfile> LoadContact(IProfile activeProfile);
    }
}