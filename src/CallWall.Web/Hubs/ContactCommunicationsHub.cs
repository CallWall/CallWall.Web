using CallWall.Web.Contracts.Communication;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactCommunications")]
    public class ContactCommunicationsHub : ObservableHub<IMessage>
    {
        public ContactCommunicationsHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<IMessage> provider) : 
            base(loggerFactory, provider)
        {}
    }
}