using System;
using CallWall.Web.EventStore.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public interface IUserContactRepository
    {
        IObservable<ContactAggregateUpdate> GetContactSummariesFrom(User user, int? versionId);
    }
}