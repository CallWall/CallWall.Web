using System;
using System.Collections.Generic;

namespace CallWall.Web.Providers
{
    public interface IContactsProvider
    {
        IObservable<IFeed<IContactSummary>> GetContactsFeed(IEnumerable<ISession> session, IEnumerable<IClientLastUpdated> lastUpdatedDetails);

        IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys);
    }
}
