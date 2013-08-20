using System;
using CallWall.Web.Models;
using CallWall.Web.Providers.Google;

namespace CallWall.Web.Providers
{
    public interface IContactsProvider
    {
        IObservable<IContactSummary> GetContacts(ISession session);
    }
}