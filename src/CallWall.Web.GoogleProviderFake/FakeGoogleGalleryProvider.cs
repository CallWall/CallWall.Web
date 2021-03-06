using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CallWall.Web.Domain;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProviderFake
{
    //TODO: Move all referenced images to Fake's content path. Have it copied on build to correct path as per other modules. -LC
    public class FakeGoogleGalleryProvider : IGalleryProvider
    {
        public IObservable<IGalleryAlbum> GetGalleryAlbums(User user, string[] contactKeys)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                .Zip(GetAlbums(), (_, msg) => msg);
        }

        private IEnumerable<GalleryAlbum> GetAlbums()
        {
            var t = DateTime.Now.Date;
            return new[]{
                new GalleryAlbum(t.AddDays(-1), t.AddDays(-1), "Interlaken Cycling", "facebook", new []{ "/Content/images/pictures/Interlaken1.jpg",
                    "/Content/images/pictures/Interlaken2.jpg",
                    "/Content/images/pictures/Interlaken3.jpg",
                    "/Content/images/pictures/Interlaken4.jpg",
                    "/Content/images/pictures/Interlaken5.jpg"}),
                new GalleryAlbum(t.AddDays(-2), t.AddDays(-2), "Landscape shots", "microsoft", new[]{
                    "/Content/images/pictures/Landscape1.jpg",
                    "/Content/images/pictures/Landscape2.jpg",
                    "/Content/images/pictures/Landscape3.jpg",
                    "/Content/images/pictures/Landscape4.jpg",
                    "/Content/images/pictures/Landscape5.jpg"
                })
            };
        }

        sealed class GalleryAlbum : IGalleryAlbum
        {
            public string[] ImageUrls { get; private set; }
            public DateTime CreatedDate { get; private set; }
            public DateTime LastModifiedDate { get; private set; }
            public string Title { get; private set; }
            public string Provider { get; private set; }

            public GalleryAlbum(DateTime createdDate, DateTime lastModifiedDate, string title, string provider, string[] imageUrls)
            {
                ImageUrls = imageUrls;
                CreatedDate = createdDate;
                LastModifiedDate = lastModifiedDate;
                Title = title;
                Provider = provider;
            }
        }
    }
}