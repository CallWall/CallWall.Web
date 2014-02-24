using System;

namespace CallWall.Web.Contracts
{
    //TODO: This really should be able to represent a timespan/period not just a point in time -LC
    public interface ICalendarEntry
    {
        DateTime Date { get; }
        string Title { get; }
    }
}