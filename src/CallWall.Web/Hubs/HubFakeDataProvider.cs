using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace CallWall.Web.Hubs
{
    //TODO: Now that we know this works, all of this code needs to be pushed to the fakes module. -LC
    //  e.g FakeGoogleContactsProvider.GetContactDetails(...)
    public class HubFakeDataProvider : IObservableHubDataProvider<CalendarEntry>,
                                       IObservableHubDataProvider<GalleryAlbum>,
                                       IObservableHubDataProvider<ContactCollaboration>
    {

        private static IObservable<T> Pump<T>(Func<IEnumerable<T>> func)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                             .Zip(func(), (_, msg) => msg);
        }

        IObservable<CalendarEntry> IObservableHubDataProvider<CalendarEntry>.GetObservable()
        {
            return Pump(GetCalendarEvents);
        }

        public IObservable<GalleryAlbum> GetObservable()
        {
            return Pump(GetAlbums);
        }
        IObservable<ContactCollaboration> IObservableHubDataProvider<ContactCollaboration>.GetObservable()
        {
            return Pump(GetContactCollaborations);
        }

        private static IEnumerable<ContactCollaboration> GetContactCollaborations()
        {
            var t = DateTime.Now.Date;
            return new[]
            {
                new ContactCollaboration("Design KO Standards", t.AddMinutes(-35), "Created Document", false, "googleDrive"),
                new ContactCollaboration("EOY 2013 Reports", t.AddDays(-8), "Modified Document", false, "googleDrive"),
                new ContactCollaboration("Pricing a cross example", t.AddDays(-37), "Modified Document", false, "googleDrive"),
                new ContactCollaboration("CallWall #122 - install Https", t.AddDays(-40), "Closed issue", true, "github"),
                new ContactCollaboration("Pricing a cross example", t.AddDays(-45), "Created document", false, "googleDrive")
            };
        }

        private IEnumerable<GalleryAlbum> GetAlbums()
        {
            var t = DateTime.Now.Date;
            return new[]{
            new GalleryAlbum(t.AddDays(-1), t.AddDays(-1), "Interlaken Cycle", "facebook", new []{ "/Content/images/pictures/Interlaken1.jpg",
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

        private static IEnumerable<CalendarEntry> GetCalendarEvents()
        {
            var t = DateTime.Now.Date;

            yield return new CalendarEntry(t.AddDays(2), "Lunch KO with Lee");
            yield return new CalendarEntry(t.AddDays(1), "Training");
            yield return new CalendarEntry(t.AddDays(0), "Document Review");
            yield return new CalendarEntry(t.AddDays(-2), "Document design session");
            yield return new CalendarEntry(t.AddDays(-3), "Lunch with Lee");
        }
    }

    public class ContactCollaboration
    {
        public string Title { get; set; }
        public DateTime ActionDate { get; set; }
        public string ActionPerformed { get; set; }
        public bool IsCompleted { get; set; }
        public string Provider { get; set; }

        public ContactCollaboration() { }

        public ContactCollaboration(string title, DateTime actionDate, string actionPerformed, bool isCompleted, string provider)
        {
            Title = title;
            ActionDate = actionDate;
            ActionPerformed = actionPerformed;
            IsCompleted = isCompleted;
            Provider = provider;
        }
    }

    public class GalleryAlbum
    {
        public string[] ImageUrls { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Title { get; set; }
        public string Provider { get; set; }

        public GalleryAlbum() { }
        public GalleryAlbum(DateTime createdDate, DateTime lastModifiedDate, string title, string provider, string[] imageUrls)
        {
            ImageUrls = imageUrls;
            CreatedDate = createdDate;
            LastModifiedDate = lastModifiedDate;
            Title = title;
            Provider = provider;
        }
    }

    public class CalendarEntry
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }

        public CalendarEntry() { }
        public CalendarEntry(DateTime date, string title)
        {
            Date = date;
            Title = title;
        }
    }
}
