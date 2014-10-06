using System;
using System.Collections.Generic;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Contact;

namespace CallWall.Web.Providers
{
    public interface IAccountContactProvider
    {
        string Provider { get; }

        IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated);

        IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys);
    }
}
