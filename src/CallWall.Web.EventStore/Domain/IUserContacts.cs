using System;

namespace CallWall.Web.EventStore.Domain
{
    public interface IUserContacts
    {
        void AddAccount(string provider, string accountId);
        void RemoveAccount(string provider, string accountId);
        void RequestRefresh();
        IObservable<IContactAggregate> GetContacts(int fromEventId);
    }
}