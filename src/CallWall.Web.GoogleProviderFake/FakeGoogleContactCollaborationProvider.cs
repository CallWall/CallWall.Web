using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProviderFake
{
    public sealed class FakeGoogleContactCollaborationProvider : IContactCollaborationProvider
    {
        public IObservable<IContactCollaboration> GetCollaborations(IEnumerable<ISession> session, string[] contactKeys)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                .Zip(GetContactCollaborations(), (_, msg) => msg);
        }

        private static IEnumerable<IContactCollaboration> GetContactCollaborations()
        {
            var t = DateTime.Now.Date;
            return new[]
            {
                new ContactCollaboration("Design Standards", t.AddMinutes(-35), "Created Document", false,
                    "googleDrive"),
                new ContactCollaboration("EOY 2013 Reports", t.AddDays(-8), "Modified Document", false, 
                    "googleDrive"),
                new ContactCollaboration("Pricing a cross example", t.AddDays(-37), "Modified Document", false,
                    "googleDrive"),
                new ContactCollaboration("CallWall #122 - install Https", t.AddDays(-40), "Closed issue", true, 
                    "github"),
                new ContactCollaboration("Pricing a cross example", t.AddDays(-45), "Created document", false,
                    "googleDrive")
            };

        }

        private sealed class ContactCollaboration : IContactCollaboration
        {
            public string Title { get; private set; }
            public DateTime ActionDate { get; private set; }
            public string ActionPerformed { get; private set; }
            public bool IsCompleted { get; private set; }
            public string Provider { get; private set; }

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
}