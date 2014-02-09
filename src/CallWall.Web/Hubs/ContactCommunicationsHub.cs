using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactCommunications")]
    public class ContactCommunicationsHub : ObservableHub<Message>
    {
        public ContactCommunicationsHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<Message> provider) : 
            base(loggerFactory, provider)
        {}
    }
}