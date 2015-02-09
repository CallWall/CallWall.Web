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
    [HubName("contactGalleryAlbums")]
    public class ContactGalleryAlbumsHub : Hub
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly IEnumerable<IGalleryProvider> _galleryProviders;
        private readonly IContactRepository _contactRepository;
        private readonly ILoginProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactGalleryAlbumsHub(IContactRepository contactRepository, IEnumerable<IGalleryProvider> galleryProviders, ILoginProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactGalleryAlbumsHub.ctor()");
            _contactRepository = contactRepository;
            _galleryProviders = galleryProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactGalleryAlbumsHub.ctor(galleryProviders:{0})", string.Join(",", _galleryProviders.Select(cp => cp.GetType().Name)));
        }

        public async Task Subscribe(string contactId)
        {
            try
            {
                Debug.Print("ContactGalleryAlbumsHub.Subscribe(...)");
                var user = await _sessionProvider.GetUser(Context.User.UserId());
                
                var query = from contactProfile in _contactRepository.GetContactDetails(user, contactId)
                            from galleryProvider in _galleryProviders
                            from album in galleryProvider.GetGalleryAlbums(user, contactProfile.ContactKeys())
                            select album;

                var subscription = query.Log(_logger, "GetGalleryAlbums")
                                        .Subscribe(album => Clients.Caller.OnNext(album),
                                                    ex => Clients.Caller.OnError("Error receiving the Gallery Albums"),
                                                    () => Clients.Caller.OnCompleted());

                _subscription.Disposable = subscription;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Requesting ContactGalleryAlbumStream in Hub.");
                Clients.Caller.OnError("Error receiving Gallery Albums");
            }
        }

        public override Task OnDisconnected()
        {
            Debug.Print("ContactGalleryAlbumsHub.OnDisconnected()");
            _subscription.Dispose();
            return base.OnDisconnected();
        }
    }
}