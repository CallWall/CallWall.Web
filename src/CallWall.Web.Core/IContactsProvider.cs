using System;
using System.Collections.Generic;

namespace CallWall.Web
{
    public interface IContactsProvider
    {
        IObservable<IFeed<IContactSummary>> GetContactsFeed(IEnumerable<ISession> session, IEnumerable<IClientLastUpdated> lastUpdatedDetails);
    }
}