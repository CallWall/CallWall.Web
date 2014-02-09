using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactCalendarEvents")]
    public class ContactCalendarEventsHub : ObservableHub<CalendarEntry>
    {
        public ContactCalendarEventsHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<CalendarEntry> provider) 
            : base(loggerFactory, provider)
        {}
    }
}