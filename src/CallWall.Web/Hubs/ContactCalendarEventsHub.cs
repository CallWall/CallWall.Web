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
        private readonly ILoginProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCalendarEventsHub(IEnumerable<ICalendarProvider> calendarProviders, ILoginProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCalendarEventsHub.ctor()");
            _calendarProviders = calendarProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCalendarEventsHub.ctor(calendarProviders:{0})", string.Join(",", _calendarProviders.Select(cp=>cp.GetType().Name)));
        }

        public async Task Subscribe(string[] contactKeys)
        {
            Debug.Print("ContactCalendarEventsHub.Subscribe(...)");
            var user = await _sessionProvider.GetUser(Context.User.UserId());
            var sessions = user.Accounts.Select(a => a.CurrentSession).ToArray();
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