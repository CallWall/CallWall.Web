using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactProfile")]
    public class ContactProfileHub : ObservableHub<IContactProfile>//TODO Object???
    {
        public ContactProfileHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<IContactProfile> provider) 
            : base(loggerFactory, provider)
        {}
    }
}