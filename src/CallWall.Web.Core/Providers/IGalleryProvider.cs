using System;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface IGalleryProvider
    {
        IObservable<IGalleryAlbum> GetGalleryAlbums(User user, string[] contactKeys);
    }
}