using System;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface IAccountContactProvider
    {
        string Provider { get; }

        IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated);

        //IObservable<IContactProfile> GetContactDetails(User user, string[] contactKeys);
    }
}
