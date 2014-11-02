using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface ICalendarProvider
    {
        IObservable<ICalendarEntry> GetCalendarEntries(IEnumerable<ISession> session, string[] contactKeys);
    }
}