using System;
using System.Collections.Generic;

namespace CallWall.Web.Providers
{
    public interface IGalleryProvider
    {
        IObservable<IGalleryAlbum> GetGalleryAlbums(IEnumerable<ISession> session, string[] contactKeys);
    }
}