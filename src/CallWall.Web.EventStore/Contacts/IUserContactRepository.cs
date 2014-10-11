using System;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public interface IUserContactRepository
    {
        IObservable<ContactAggregateUpdate> GetContactSummariesFrom(User user, int? versionId);
        IObservable<int> ObserveContactUpdatesHeadVersion(User user);
    }
}