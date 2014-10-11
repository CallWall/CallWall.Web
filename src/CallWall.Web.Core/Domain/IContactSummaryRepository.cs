using System;

namespace CallWall.Web.Domain
{
    public interface IContactSummaryRepository
    {
        IObservable<ContactAggregateUpdate> GetContactUpdates(User user, int fromEventId);

        IObservable<int> ObserveContactUpdatesHeadVersion(User user);
    }
}