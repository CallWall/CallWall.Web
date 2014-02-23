using System;
using System.Collections.Generic;
using CallWall.Web.Contracts;

namespace CallWall.Web.Providers
{
    public interface ICalendarProvider
    {
        IObservable<ICalendarEntry> GetCalendarEntries(IEnumerable<ISession> session, string[] contactKeys);
    }
}