using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface IAccountContactProvider
    {
        string Provider { get; }

        IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated);

        IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys);
    }
}
