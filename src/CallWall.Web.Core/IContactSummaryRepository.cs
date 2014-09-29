using System;

namespace CallWall.Web
{
    public interface IContactSummaryRepository
    {
        IObservable<ContactAggregateUpdate> GetContactUpdates(User user, int fromEventId);
    }
}