using System;

namespace CallWall.Web.Domain
{
    public interface IContactRepository
    {
        IObservable<int> ObserveContactUpdatesHeadVersion(User user);

        IObservable<Event<ContactAggregateUpdate>> GetContactUpdates(User user, int fromEventId);

        
    }
}