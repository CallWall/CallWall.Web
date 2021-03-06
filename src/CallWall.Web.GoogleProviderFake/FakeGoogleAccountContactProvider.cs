﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using CallWall.Web.Domain;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleAccountContactProvider : IAccountContactProvider
    {
        public string Provider { get { return Constants.ProviderName; } }

        public IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            return GetContactSummaries();
        }

        public IObservable<IContactProfile> GetContactDetails(User user, string[] contactKeys)
        {
            //return Observable.Empty<IContactProfile>();
            return Observable.Interval(TimeSpan.FromSeconds(1))
                             .Zip(Profiles(), (_, msg) => msg);
        }

        private static IEnumerable<IContactProfile> Profiles()
        {
            yield return new ContactProfile
            {
                Id=1,
                Title = "Lee HUB Campbell",
                //fullName = "",
                DateOfBirth = new DateTime(1979, 12, 25),
                Tags = new[] { "Family", "Dolphins", "London" },
                Organizations = new[]
                            {
                                new ContactAssociation("Consultant", "Adaptive"),
                                new ContactAssociation("Triathlon", "Serpentine")
                            },
                Relationships = new[] 
                    { 
                        new ContactAssociation("Wife", "Erynne"), 
                        new ContactAssociation("Brother", "Rhys") 
                    },
                Handles = new ContactHandle[]
                            {
                                new ContactPhoneNumber("07827743025","Mobile - UK"), 
                                new ContactPhoneNumber("021 254 3824", "Mobile - NZ"),
                            
                                new ContactEmailAddress("lee.ryan.campbell@gmail.com", "Home"),
                                new ContactEmailAddress("lee.campbell@callwall.com", "Work")
                            },
            };
            yield return new ContactProfile
            {
                Id = 2,
                //title = "Lee Campbell",
                FullName = "Mr. Lee Ryan Campbell",
                DateOfBirth = new DateTime(1979, 12, 27),
                AvatarUris = new[] { "/Content/images/pictures/Interlaken1.jpg" },
                Tags = new[] { "Adaptive", "Serpentine", "ReactConf", "Amazon", "Turtle" },
                Organizations = new[] { new ContactAssociation("CEO", "CallWall") },
                Relationships = new[] { new ContactAssociation("CFO", "John Bell"), },
            };
        }

        private const string Family = "Family";
        private const string Colleague = "Colleague";
        private const string Dolphins = "Dolphins";

        private IObservable<IAccountContactSummary> GetContactSummaries()
        {
            return Observable.Interval(TimeSpan.FromSeconds(0.25))
                .Zip(
                    new[]
                    {
                        new ContactSummary {ProviderId = "1", Title = "Lee Campbell", Handles = new[]{new ContactEmailAddress("lee@mail.com", "Home"), }},
                        new ContactSummary {ProviderId = "2", Title = "Rhys Campbell", Tags = new[] {Family,Colleague,Dolphins}},
                        new ContactSummary {ProviderId = "3", Title = "Erynne Campbell", Tags = new[] {Family,Dolphins}},
                        new ContactSummary {ProviderId = "4", Title = "Tori Campbell", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "5", Title = "Jim Amm", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "6", Title = "Gaye Amm", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "7", Title = "Karen Morrogh", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "8", Title = "Grant Morrogh", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "9", Title = "Greg Fox", Tags = new[] {Colleague}},
                        new ContactSummary {ProviderId = "10", Title = "Matt Barrett", Tags = new[] {Colleague}},
                        new ContactSummary {ProviderId = "11", Title = "Jake Ginnivan", Tags = new[] {Colleague}},
                        new ContactSummary {ProviderId = "12", Title = "Paul Spiteri", Tags = new[] {Colleague}},
                        new ContactSummary {ProviderId = "13", Title = "Steph Campbell", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "14", Title = "Chris Campbell", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "15", Title = "Colin Campbell", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "16", Title = "Linda Campbell", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "17", Title = "Martine Harris", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "18", Title = "John Harris", Tags = new[] {Family}},
                        new ContactSummary {ProviderId = "19", Title = "John Marks", Tags = new[] {Colleague}},
                        new ContactSummary {ProviderId = "20", Title = "Loic Roze", Tags = new[] {Colleague}},
                        new ContactSummary {ProviderId = "21", Title = "Olivier Dehuerles", Tags = new[] {Colleague}},
                        new ContactSummary {ProviderId = "22", Title = "John Bell", Tags = new[] {Dolphins}},
                        new ContactSummary {ProviderId = "23", Title = "Nick Gianco", Tags = new[] {Colleague}},
                        new ContactSummary {ProviderId = "24", Title = "Nick Harlin", Tags = new[] {Dolphins}},
                        new ContactSummary {ProviderId = "25", Title = "Mark Entwistle", Tags = new[] {Dolphins}},
                        new ContactSummary {ProviderId = "26", Title = "Mark Ridgewell", Tags = new[] {Dolphins}},

                    }.ToObservable(), (_, c) => c)
                .Concat(Observable.Throw<IAccountContactSummary>(new IOException("Fake error")));
        }

        private sealed class ContactSummary : IAccountContactSummary
        {
            public ContactSummary()
            {
                Tags = Enumerable.Empty<string>();
                Handles = Enumerable.Empty<ContactHandle>();
            }
            public bool IsDeleted { get { return false; } }
            public string Provider { get { return Constants.ProviderName; } }

            public string AccountId { get { return "lee.fake@gmail.com"; } }

            public string ProviderId { get; set; }
            public string Title { get; set; }
            public IAnniversary DateOfBirth { get; private set; }
            public string FullName { get; private set; }
            public IEnumerable<string> AvatarUris { get { return Enumerable.Empty<string>(); } }
            public IEnumerable<string> Tags { get; set; }
            public IEnumerable<ContactHandle> Handles { get; set; }
            public IEnumerable<IContactAssociation> Organizations { get { return Enumerable.Empty<IContactAssociation>(); } }
            public IEnumerable<IContactAssociation> Relationships { get { return Enumerable.Empty<IContactAssociation>(); } }
        }

        private sealed class ContactProfile : IContactProfile
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string FullName { get; set; }
            public IEnumerable<string> AvatarUris { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public IEnumerable<IContactAssociation> Organizations { get; set; }
            public IEnumerable<IContactAssociation> Relationships { get; set; }
            public IEnumerable<ContactHandle> Handles { get; set; }
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
