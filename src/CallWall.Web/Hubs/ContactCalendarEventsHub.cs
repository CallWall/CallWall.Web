using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.Providers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactCalendarEvents")]
    public class ContactCalendarEventsHub : Hub
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly IEnumerable<ICalendarProvider> _calendarProviders;
        private readonly ISessionProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCalendarEventsHub(IEnumerable<ICalendarProvider> calendarProviders, ISessionProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCalendarEventsHub.ctor()");
            _calendarProviders = calendarProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCalendarEventsHub.ctor(calendarProviders:{0})", string.Join(",", _calendarProviders.Select(cp=>cp.GetType().Name)));
        }

        public void Subscribe(string[] contactKeys)
        {
            Debug.Print("ContactProfileHub.Subscribe(...)");
            var sessions = _sessionProvider.GetSessions(Context.User);
            var subscription = _calendarProviders
                                .ToObservable()
                                .SelectMany(c => c.GetCalendarEntries(sessions, contactKeys))
                                .Log(_logger, "GetCalendarEntries")
                                .Subscribe(contact => Clients.Caller.OnNext(contact),
                                           ex => Clients.Caller.OnError("Error receiving the Calendar"),
                                           () => Clients.Caller.OnCompleted());

            _subscription.Disposable = subscription;
        }

        public override Task OnDisconnected()
        {
            Debug.Print("ContactCalendarEventsHub.OnDisconnected()");
            _subscription.Dispose();
            return base.OnDisconnected();
        }
    }
}