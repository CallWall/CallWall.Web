using System;

namespace CallWall.Web
{
    public interface IContactSummaryRepository
    {
        IObservable<IContactSummaryUpdate> GetContactUpdates(int userId, int fromEventId);
    }
}