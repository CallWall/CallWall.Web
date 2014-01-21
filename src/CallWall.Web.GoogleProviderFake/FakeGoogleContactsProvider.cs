using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleContactsProvider : IContactsProvider
    {
        public IObservable<IFeed<IContactSummary>> GetContactsFeed(IEnumerable<ISession> sessions, IEnumerable<IClientLastUpdated> lastUpdatedDetails)
        {
            return Observable.Return(new ContactFeed());
        }

        public IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session)
        {
            var result = new ContactProfile
            {
                Title = "Lee Campbell",
                FullName = "Mr. Lee Ryan Campbell",
                DateOfBirth = new DateTime(1979, 12, 31),
                //Avatars = 
                Tags = new[] { "Family", "Water polo"},
                Organizations = new[]
                {
                    new ContactAssociation{Association = "Owner", Name = "CallWall"}, 
                    new ContactAssociation{Association = "Consultant", Name = "Adaptive"}, 
                    new ContactAssociation{Association = "Member", Name = "Dolphins"}, 
                },

                Relationships= new[]
                {
                    new ContactAssociation{Association = "Wife", Name = "Erynne"}, 
                    new ContactAssociation{Association = "Brother", Name = "Rhys"}, 
                },
                EmailAddresses = new[]
                {
                    new ContactAssociation{Association = "work", Name = "Lee.Campbell@callwall.com"}, 
                },
                PhoneNumbers= new[]
                {
                    new ContactAssociation{Association = "work", Name = "+44 7 4321 4321"}, 
                },
            
            };
            return Observable.Return(result);
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

        public class ContactSummary : IContactSummary
        {
            public string Provider { get; private set; }
            public string ProviderId { get; private set; }
            public string Title { get; set; }
            public string PrimaryAvatar { get; set; }
            public IEnumerable<string> Tags { get; set; }
        }

        public class ContactProfile : IContactProfile
        {
            public string Title { get; set; }
            public string FullName { get; set; }
            public IEnumerable<Uri> Avatars { get;  set; }
            public DateTime? DateOfBirth { get;  set; }
            public IEnumerable<string> Tags { get;  set; }
            public IEnumerable<IContactAssociation> Organizations { get; set; }
            public IEnumerable<IContactAssociation> Relationships { get; set; }
            public IEnumerable<IContactAssociation> EmailAddresses { get; set; }
            public IEnumerable<IContactAssociation> PhoneNumbers { get; set; }
        }

        public class ContactAssociation : IContactAssociation
        {
            public string Name { get; set; }
            public string Association { get; set; }
        }
    }
} 
