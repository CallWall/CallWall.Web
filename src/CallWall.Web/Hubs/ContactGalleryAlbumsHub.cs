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
    [HubName("contactGalleryAlbums")]
    public class ContactGalleryAlbumsHub :  Hub
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly IEnumerable<IGalleryProvider> _galleryProviders;
        private readonly ILoginProvider _sessionProvider;
        private readonly ILogger _logger;

        public ContactGalleryAlbumsHub(IEnumerable<IGalleryProvider> galleryProviders, ILoginProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactGalleryAlbumsHub.ctor()");
            _galleryProviders = galleryProviders.ToArray();
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Trace("ContactGalleryAlbumsHub.ctor(galleryProviders:{0})", string.Join(",", _galleryProviders.Select(cp => cp.GetType().Name)));
        }

        public async Task Subscribe(string[] contactKeys)
        {
            Debug.Print("ContactGalleryAlbumsHub.Subscribe(...)");
            var user = await _sessionProvider.GetUser(Context.User.UserId());
            var sessions = user.Accounts.Select(a => a.CurrentSession).ToArray();
            var subscription = _galleryProviders
                                .ToObservable()
                                .SelectMany(c => c.GetGalleryAlbums(sessions, contactKeys))
                                .Log(_logger, "GetGalleryAlbums")
                                .Subscribe(contact => Clients.Caller.OnNext(contact),
                                           ex => Clients.Caller.OnError("Error receiving the Gallery Albums"),
                                           () => Clients.Caller.OnCompleted());

            _subscription.Disposable = subscription;
        }

        public override Task OnDisconnected()
        {
            Debug.Print("ContactGalleryAlbumsHub.OnDisconnected()");
            _subscription.Dispose();
            return base.OnDisconnected();
        }
    }
}