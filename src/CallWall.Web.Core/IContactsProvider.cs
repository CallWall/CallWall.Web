using System;

namespace CallWall.Web
{
    public interface IContactsProvider
    {
        IObservable<IFeed<IContactSummary>> GetContactsFeed(ISession session);
    }
}