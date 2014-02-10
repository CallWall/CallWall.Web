using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace CallWall.Web.Hubs
{
    //TODO: Now that we know this works, all of this code needs to be pushed to the fakes module. -LC
    //  e.g FakeGoogleContactsProvider.GetContactDetails(...)
    public class HubFakeDataProvider : IObservableHubDataProvider<ContactCollaboration>
    {

        private static IObservable<T> Pump<T>(Func<IEnumerable<T>> func)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                             .Zip(func(), (_, msg) => msg);
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
}
