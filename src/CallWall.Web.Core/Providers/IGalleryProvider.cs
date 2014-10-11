using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public interface IGalleryProvider
    {
        IObservable<IGalleryAlbum> GetGalleryAlbums(IEnumerable<ISession> session, string[] contactKeys);
    }
}