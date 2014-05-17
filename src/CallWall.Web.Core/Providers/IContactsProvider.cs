using System;
using System.Collections.Generic;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Contact;

namespace CallWall.Web.Providers
{
    public interface IContactsProvider
    {
        IObservable<IFeed<IContactSummary>> GetContactsFeed(ISession session, DateTime lastUpdated);

        IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys);
    }
}
