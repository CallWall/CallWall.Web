using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactCollaborations")]
    public class ContactCollaborationHub : ObservableHub<ContactCollaboration>
    {
        public ContactCollaborationHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<ContactCollaboration> provider) 
            : base(loggerFactory, provider)
        {}
    }
}