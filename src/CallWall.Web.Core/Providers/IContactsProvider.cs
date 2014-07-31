using System;
using System.Collections.Generic;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Contact;

namespace CallWall.Web.Providers
{
    //TODO: Rename to IAccountContactsProvider -LC
    public interface IContactsProvider
    {
        IObservable<IFeed<IContactSummary>> GetContactsFeed(IAccount account, DateTime lastUpdated);

        IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys);
    }
}
