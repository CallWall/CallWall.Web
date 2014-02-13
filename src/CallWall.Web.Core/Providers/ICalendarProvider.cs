using System;
using System.Collections.Generic;

namespace CallWall.Web.Providers
{
    public interface ICalendarProvider
    {
        IObservable<ICalendarEntry> GetCalendarEntries(IEnumerable<ISession> session, string[] contactKeys);
    }
}