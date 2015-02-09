using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.Domain;
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
        private readonly IContactRepository _contactRepository;
        private readonly ILoginProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactCalendarEventsHub(IContactRepository contactRepository, IEnumerable<ICalendarProvider> calendarProviders, ILoginProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactCalendarEventsHub.ctor()");
            _contactRepository = contactRepository;
            _calendarProviders = calendarProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactCalendarEventsHub.ctor(calendarProviders:{0})", string.Join(",", _calendarProviders.Select(cp=>cp.GetType().Name)));
        }

        public async Task Subscribe(string contactId)
        {
            Debug.Print("ContactCalendarEventsHub.Subscribe(...)");
            var user = await _sessionProvider.GetUser(Context.User.UserId());

            var query = from contactProfile in _contactRepository.GetContactDetails(user, contactId)
                        from calendarProvider in _calendarProviders
                        from calendarEntry in calendarProvider.GetCalendarEntries(user, contactProfile.ContactKeys())
                        select calendarEntry;

            var subscription = query.Log(_logger, "GetCalendarEntries")
                                    .Subscribe(calendarEntry => Clients.Caller.OnNext(calendarEntry),
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