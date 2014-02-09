using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactCalendarEvents")]
    public class ContactCalendarEventsHub : Hub
    {
        private readonly ILogger _logger;
        private readonly SerialDisposable _contactComunicationSubscription = new SerialDisposable();

        public ContactCalendarEventsHub(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void Subscribe(string[] contactKeys)
        {
            //TODO: Replace with Rx Logger --https://gist.github.com/LeeCampbell/3817281
            try
            {
                _logger.Debug("ContactCalendarEventsHub.Subscribe({0})", string.Join(",", contactKeys));
                var subscription = Observable.Interval(TimeSpan.FromSeconds(2))
                    .Zip(GetCalendarEvents(), (_, msg) => msg)
                    .Subscribe(
                        message => Clients.Caller.OnNext(message),
                        ex =>
                        {
                            _logger.Error(ex, "Error in getting calendar events");
                            Clients.Caller.OnError("Error in getting calendar events");
                        },
                        () => Clients.Caller.OnCompleted());
                _contactComunicationSubscription.Disposable = subscription;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ContactCalendarEventsHub failure");
                Clients.Caller.OnError("Error in getting calendar events");
            }
        }

        private static IEnumerable<CalendarEntry> GetCalendarEvents()
        {
            var t = DateTime.Now.Date;

            yield return new CalendarEntry(t.AddDays(2), "Lunch KO with Lee");
            yield return new CalendarEntry(t.AddDays(1), "Training");
            yield return new CalendarEntry(t.AddDays(0), "Document Review");
            yield return new CalendarEntry(t.AddDays(-2), "Document design session");
            yield return new CalendarEntry(t.AddDays(-3), "Lunch with Lee");
        }

    }
    public class CalendarEntry
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }

        public CalendarEntry() { }
        public CalendarEntry(DateTime date, string title)
        {
            Date = date;
            Title = title;
        }
    }
}

