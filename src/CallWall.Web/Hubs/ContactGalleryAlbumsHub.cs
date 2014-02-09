using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactGalleryAlbums")]
    public class ContactGalleryAlbumsHub : ObservableHub<GalleryAlbum>
    {
        public ContactGalleryAlbumsHub(ILoggerFactory loggerFactory, IObservableHubDataProvider<GalleryAlbum> provider) 
            : base(loggerFactory, provider)
        {}
    }
}