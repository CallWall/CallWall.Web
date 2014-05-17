using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using CallWall.Web.Providers;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Contact;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleContactsProvider : IContactsProvider
    {
        public IObservable<IFeed<IContactSummary>> GetContactsFeed(ISession session, DateTime lastUpdated)
        {
            return Observable.Return(new ContactFeed());
        }

        public IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                             .Zip(Profiles(), (_, msg) => msg);
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

        private sealed class ContactFeed : IFeed<IContactSummary>
        {
            private const string Family = "Family";
            private const string Colleague = "Colleague";
            private const string Dolphins = "Dolphins";

            public ContactFeed()
            {
                var contacts = new[]
                {
                    new ContactSummary {Title = "Lee Campbell"},
                    new ContactSummary {Title = "Rhys Campbell", Tags = new[] {Family,Colleague,Dolphins}},
                    new ContactSummary {Title = "Erynne Campbell", Tags = new[] {Family,Dolphins}},
                    new ContactSummary {Title = "Tori Campbell", Tags = new[] {Family}},
                    new ContactSummary {Title = "Jim Amm", Tags = new[] {Family}},
                    new ContactSummary {Title = "Gaye Amm", Tags = new[] {Family}},
                    new ContactSummary {Title = "Karen Morrogh", Tags = new[] {Family}},
                    new ContactSummary {Title = "Grant Morrogh", Tags = new[] {Family}},
                    new ContactSummary {Title = "Greg Fox", Tags = new[] {Colleague}},
                    new ContactSummary {Title = "Matt Barrett", Tags = new[] {Colleague}},
                    new ContactSummary {Title = "Jake Ginnivan", Tags = new[] {Colleague}},
                    new ContactSummary {Title = "Paul Spiteri", Tags = new[] {Colleague}},
                    new ContactSummary {Title = "Steph Campbell", Tags = new[] {Family}},
                    new ContactSummary {Title = "Chris Campbell", Tags = new[] {Family}},
                    new ContactSummary {Title = "Colin Campbell", Tags = new[] {Family}},
                    new ContactSummary {Title = "Linda Campbell", Tags = new[] {Family}},
                    new ContactSummary {Title = "Martine Harris", Tags = new[] {Family}},
                    new ContactSummary {Title = "John Harris", Tags = new[] {Family}},
                    new ContactSummary {Title = "John Marks", Tags = new[] {Colleague}},
                    new ContactSummary {Title = "Loic Roze", Tags = new[] {Colleague}},
                    new ContactSummary {Title = "Olivier Dehuerles", Tags = new[] {Colleague}},
                    new ContactSummary {Title = "John Bell", Tags = new[] {Dolphins}},
                    new ContactSummary {Title = "Nick Gianco", Tags = new[] {Colleague}},
                    new ContactSummary {Title = "Nick Harlin", Tags = new[] {Dolphins}},
                    new ContactSummary {Title = "Mark Entwistle", Tags = new[] {Dolphins}},
                    new ContactSummary {Title = "Mark Ridgewell", Tags = new[] {Dolphins}},

                };
                TotalResults = contacts.Length;
                Values = Observable.Interval(TimeSpan.FromSeconds(0.25))
                                   .Zip(contacts.ToObservable(), (_, c) => c)
                                   .Concat(Observable.Throw<IContactSummary>(new IOException("Fake error")));
            }
            public int TotalResults { get; private set; }
            public IObservable<IContactSummary> Values { get; private set; }
        }

        private sealed class ContactSummary : IContactSummary
        {
            public string Provider { get; private set; }
            public string ProviderId { get; private set; }
            public string Title { get; set; }
            public string PrimaryAvatar { get; set; }
            public IEnumerable<string> Tags { get; set; }
        }

        private sealed class ContactProfile : IContactProfile
        {
            public string Title { get; set; }
            public string FullName { get; set; }
            public IEnumerable<Uri> Avatars { get; set; }//TODO: Not set or read from anywhere yet. -LC
            public DateTime? DateOfBirth { get;  set; }
            public IEnumerable<string> Tags { get;  set; }
            public IEnumerable<IContactAssociation> Organizations { get; set; }
            public IEnumerable<IContactAssociation> Relationships { get; set; }
            public IEnumerable<IContactAssociation> EmailAddresses { get; set; }
            public IEnumerable<IContactAssociation> PhoneNumbers { get; set; }
        }

        private sealed class ContactAssociation : IContactAssociation
        {
            public ContactAssociation(string name, string association)
            {
                Name = name;
                Association = association;
            }

            public string Name { get; private set; }
            public string Association { get; private set; }
        }
    }
} 
