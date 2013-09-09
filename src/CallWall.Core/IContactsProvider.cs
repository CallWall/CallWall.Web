using System;

namespace CallWall.Web
{
    public interface IContactsProvider
    {
        IObservable<IContactSummary> GetContacts(ISession session);
    }
}