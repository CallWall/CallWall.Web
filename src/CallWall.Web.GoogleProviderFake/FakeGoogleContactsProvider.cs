using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleContactsProvider : IContactsProvider
    {
        public IObservable<IFeed<IContactSummary>> GetContactsFeed(ISession session)
        {
            return Observable.Return(new ContactFeed());
        }

        private sealed class ContactFeed : IFeed<IContactSummary>
        {
            public ContactFeed()
            {
                var contacts = new[]
                {
                    new ContactSummary {Title = "Lee Campbell"},
                    new ContactSummary {Title = "Rhys Campbell"},
                    new ContactSummary {Title = "Erynne Campbell"},
                    new ContactSummary {Title = "Tori Campbell"},
                };
                TotalResults = contacts.Length;
                Values = Observable.Interval(TimeSpan.FromSeconds(1))
                                   .Zip(contacts.ToObservable(), (_, c) => c)
                                   .Concat(Observable.Throw<IContactSummary>(new IOException("Fake error")));
            }
            public int TotalResults { get; private set; }
            public IObservable<IContactSummary> Values { get; private set; }
        }

        public class ContactSummary : IContactSummary
        {
            public string Title { get; set; }
            public string PrimaryAvatar { get; set; }
            public IEnumerable<string> Tags { get; set; }
        }
    }
}