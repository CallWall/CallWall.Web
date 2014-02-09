using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactProfile")]
    public class ContactProfileHub : ObservableHub<object>//TODO Object???
    {
        public ContactProfileHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<object> provider) 
            : base(loggerFactory, provider)
        {}
    }
}