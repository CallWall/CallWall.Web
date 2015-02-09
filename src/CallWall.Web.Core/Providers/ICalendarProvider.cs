using System;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface ICalendarProvider
    {
        IObservable<ICalendarEntry> GetCalendarEntries(User user, string[] contactKeys);
    }
}