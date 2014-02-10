using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleCalendarProvider : ICalendarProvider
    {
        public IObservable<ICalendarEntry> GetCalendarEntries(IEnumerable<ISession> session, string[] contactKeys)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                .Zip(GetCalendarEvents(), (_, msg) => msg);
        }

        private static IEnumerable<ICalendarEntry> GetCalendarEvents()
        {
            var t = DateTime.Now.Date;

            yield return new CalendarEntry(t.AddDays(2), "Lunch with Lee");
            yield return new CalendarEntry(t.AddDays(1), "Training");
            yield return new CalendarEntry(t.AddDays(0), "Document Review");
            yield return new CalendarEntry(t.AddDays(-2), "Document design session");
            yield return new CalendarEntry(t.AddDays(-3), "Lunch with Lee");
        }

        sealed class CalendarEntry : ICalendarEntry
        {
            public DateTime Date { get; private set; }
            public string Title { get; private set; }

            public CalendarEntry(DateTime date, string title)
            {
                Date = date;
                Title = title;
            }
        }
    }
}