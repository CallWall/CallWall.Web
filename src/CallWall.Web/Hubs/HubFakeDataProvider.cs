using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace CallWall.Web.Hubs
{
    public class HubFakeDataProvider : IObservableHubDataProvider<CalendarEntry>,
                                       IObservableHubDataProvider<IContactProfile>,
                                       IObservableHubDataProvider<Message>
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

        IObservable<IContactProfile> IObservableHubDataProvider<IContactProfile>.GetObservable()
        {
            return Pump(Profiles);
        }
        IObservable<Message> IObservableHubDataProvider<Message>.GetObservable()
        {
            return Pump(GetMessages);
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
        private static IEnumerable<Message> GetMessages()
        {
            var n = DateTime.Now;
            yield return new Message(n.AddMinutes(-10), false, "On my way", null, "hangouts");
            yield return new Message(n.AddMinutes(-13), true, "Dude, where are you?", null, "hangouts");
            yield return new Message(n.AddDays(-2), false, "Pricing a cross example", "Here is the sample we were talking about the other day. It should cover the basic case, the complex multi-leg option case and all the variations in-between. If you have any questions, then just email me back on my home account.", "linkedin");
            yield return new Message(n.AddDays(-4), false, "I will bring the food for the Rugby", "From: James Alex To: You, Lee FAKE Camplell, Simon Real, Brian Baxter, Josh Taylor and Sally Hubbard", "gmail");
            yield return new Message(n.AddDays(-4), false, "CallWall are recruiting engineers now!", "Retweets : 7", "twitter");
            yield return new Message(n.AddDays(-5), true, "Rugby at my place on Saturday morning", "To: James Alex, Simon Real + 3 others", "gmail");
        }
        private static IEnumerable<IContactProfile> Profiles()
        {
            yield return new ContactProfile
            {
                Title = "Lee HUB Campbell",
                //fullName = "",
                DateOfBirth = new DateTime(1979, 12, 25),
                Tags = new[] { "Family", "Dolphins", "London" },
                Organizations =
                    new[]
                            {
                                new ContactAssociation("Consultant", "Adaptive"),
                                new ContactAssociation("Triathlon", "Serpentine")
                            },
                Relationships =
                    new[] { new ContactAssociation("Wife", "Erynne"), new ContactAssociation("Brother", "Rhys") },
                PhoneNumbers =
                    new[]
                            {
                                new ContactAssociation("Mobile - UK", "07827743025"),
                                new ContactAssociation("Mobile - NZ", "021 254 3824")
                            },
                EmailAddresses =
                    new[]
                            {
                                new ContactAssociation("Home", "lee.ryan.campbell@gmail.com"),
                                new ContactAssociation("Work", "lee.campbell@callwall.com")
                            },
            };
            yield return new ContactProfile
            {
                //title = "Lee Campbell",
                FullName = "Mr. Lee Ryan Campbell",
                DateOfBirth = new DateTime(1979, 12, 27),
                Tags = new[] { "Adaptive", "Serpentine", "ReactConf", "Amazon", "Turtle" },
                Organizations = new[] { new ContactAssociation("CEO", "CallWall") },
                Relationships = new[] { new ContactAssociation("CFO", "John Bell"), },
            };
        }
    }

    public class ContactProfile : IContactProfile
    {
        public string Title { get; set; }
        public string FullName { get; set; }
        public IEnumerable<Uri> Avatars { get; set; }   //TODO: Not set or read from anywhere yet. -LC
        public DateTime? DateOfBirth { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<IContactAssociation> Organizations { get; set; }
        public IEnumerable<IContactAssociation> Relationships { get; set; }
        public IEnumerable<IContactAssociation> PhoneNumbers { get; set; }
        public IEnumerable<IContactAssociation> EmailAddresses { get; set; }
        
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
    public class ContactAssociation : IContactAssociation
    {
        public ContactAssociation(string name, string association)
        {
            Name = name;
            Association = association;
        }

        public string Name { get; private set; }

        public string Association { get; private set; }
    }
    public class Message
    {
        public DateTime Timestamp { get; set; }
        public bool IsOutbound { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Provider { get; set; }

        public Message() { }
        public Message(DateTime timestamp, bool isOutbound, string subject, string content, string provider)
        {
            Timestamp = timestamp;
            IsOutbound = isOutbound;
            Subject = subject;
            Content = content;
            Provider = provider;
        }
    }
}