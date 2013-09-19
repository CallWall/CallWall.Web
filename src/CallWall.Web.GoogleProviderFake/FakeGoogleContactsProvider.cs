using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleContactsProvider : IContactsProvider
    {
        public IObservable<IContactSummary> GetContacts(ISession session)
        {
            var contacts = new[]
                {
                    new ContactSummary {Title = "Lee Campbell"},
                    new ContactSummary {Title = "Rhys Campbell"},
                    new ContactSummary {Title = "Erynne Campbell"},
                    new ContactSummary {Title = "Tori Campbell"},
                };

            return Observable.Interval(TimeSpan.FromSeconds(1))
                             .Zip(contacts.ToObservable(), (_, c) => c)
                             .Concat(Observable.Throw<IContactSummary>(new IOException("Fake error")));
        }
    }

    public class ContactSummary : IContactSummary
    {
        public string Title { get; set; }
        public string PrimaryAvatar { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}