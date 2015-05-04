using System;

namespace CallWall.Web.Domain
{
    public interface IContactFeedRepository
    {
        IObservable<int> ObserveContactUpdatesHeadVersion(User user);

        IObservable<Event<ContactAggregateUpdate>> GetContactUpdates(User user, int fromEventId);
    }
}