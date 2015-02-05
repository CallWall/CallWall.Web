using System;
using System.Linq;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Contacts;
using CallWall.Web.EventStore.Tests.Doubles;
using NUnit.Framework;
using TestStack.BDDfy;

namespace CallWall.Web.EventStore.Tests
{
    [TestFixture]
    [Story(Title = "User contacts are updated",
        AsA = "As an end user",
        IWant = "I want to have changes to my contact data ",
        SoThat = "So that I can receive changes faster over poor network conditions")]
    [Timeout(1000)]
    public class UserContactsUpdatedFixture
    {
        [Test]
        public void AddingNewContact()
        {
            var contact = GenerateContact("PrimaryAccount", "John Doe", "Stub-John@doe.com", "John@doe.com");
            new UserContactUpdateSingleContactAggregateScenario()
               .Given(s => s.Given_a_UserContacts_instance())
               .When(s => s.When_a_ContactSummary_is_added(contact))
               .Then(s => s.Then_Snapshot_includes_contact(c => c.NewTitle == contact.Title
                   && c.AddedProviders.Single().ProviderName == contact.Provider
                   && c.AddedProviders.Single().AccountId == contact.AccountId
                   && c.AddedProviders.Single().ContactId == contact.ProviderId
                   ))
               .BDDfy();
        }

        [Test]
        public void UpdatingContact()
        {
            var contact = GenerateContact("PrimaryAccount", "John Doe", "Stub-John@doe.com", "John@doe.com");
            var updated = contact.Clone();
            updated.Title = "Jonny Doe";
            new UserContactUpdateSingleContactAggregateScenario()
               .Given(s => s.Given_a_UserContacts_instance())
               .When(s => s.When_a_ContactSummary_is_added(contact))
               .When(s => s.When_a_ContactSummary_is_added(updated))
               .Then(s => s.Then_Snapshot_includes_contact(c => c.NewTitle == updated.Title
                   && c.AddedProviders.Single().ProviderName == updated.Provider
                   && c.AddedProviders.Single().AccountId == updated.AccountId
                   && c.AddedProviders.Single().ContactId == updated.ProviderId
                   ))
               .BDDfy();
        }

        [Test]
        public void RemovingContact()
        {
            var contactA = GenerateContact("PrimaryAccount", "Alex Albert", "Stub-AlexA@mail.com", "AlexA@mail.com");
            var contactB = GenerateContact("PrimaryAccount", "Barry Bonds", "Stub-Barry.Bonds@gmail.com", "Barry.Bonds@gmail.com");
            var contactC = GenerateContact("PrimaryAccount", "Charlie Campbell", "Stub-Chuck@Campbell.com", "Chuck@Campbell.com");

            var contactBDeletion = contactB.Clone();
            contactBDeletion.IsDeleted = true;

            var expected = new ContactAggregateUpdate
            {
                Version = 2,
                IsDeleted = true,
            };

            new UserContactRemovedScenario()
               .Given(s => s.Given_a_populated_UserContacts_instance(contactA, contactB, contactC))
               .When(s => s.When_a_ContactSummary_is_removed(contactBDeletion))
               .Then(s => s.Then_Snapshot_has_only(expected))
               .BDDfy();
        }

        [Test]
        public void UpdatingContactAggregateWithMatchingContact()
        {
            var initialContact = new StubContactSummary
            {
                Provider = "StubProvider",
                AccountId = "Home@mail.com",
                ProviderId = "Stub-1-John@doe.com",
                Title = "Jonny Doe",
                Handles =
                {
                    new ContactEmailAddress("John@doe.com", "home"),
                }
            };
            var matchingContact = new StubContactSummary
            {
                Provider = "StubProvider",
                AccountId = "Work@mail.com",
                ProviderId = "Stub-2-John@doe.com",
                Handles =
                {
                    new ContactEmailAddress("John@doe.com", "other"),
                    new ContactPhoneNumber("+44 7 1234 56778", "mobile"),
                }
            };

            var expected = new ContactAggregateUpdate
            {
                Version = 2,
                NewTitle = initialContact.Title,
                AddedProviders = new[]
                {
                    new ContactProviderSummary(initialContact.Provider, initialContact.AccountId,
                        initialContact.ProviderId),
                    new ContactProviderSummary(matchingContact.Provider, matchingContact.AccountId,
                        matchingContact.ProviderId),
                },
                AddedHandles = initialContact.Handles.Concat(matchingContact.Handles.Skip(1)).ToArray()
            };

            new UserContactUpdateSingleContactAggregateScenario()
               .Given(s => s.Given_a_UserContacts_instance())
               .When(s => s.When_a_ContactSummary_is_added(initialContact))
               .When(s => s.When_a_ContactSummary_is_added(matchingContact))
               .Then(s => s.Then_Snapshot_has_only(expected))
               .BDDfy();
        }

        [TestCase(null)]
        [TestCase("")]
        public void Adding_contact_with_no_title_should_use_email_as_title(string title)
        {
            var emailHandle = "John@doe.com";
            var contact = GenerateContact("PrimaryAccount", title, "Stub-John@doe.com", emailHandle);
            new UserContactUpdateSingleContactAggregateScenario()
               .Given(s => s.Given_a_UserContacts_instance())
               .When(s => s.When_a_ContactSummary_is_added(contact))
               .Then(s => s.Then_Snapshot_includes_contact(c =>
                   c.NewTitle == emailHandle
                   && c.AddedProviders.Single().ProviderName == contact.Provider
                   && c.AddedProviders.Single().AccountId == contact.AccountId
                   && c.AddedProviders.Single().ContactId == contact.ProviderId))
               .BDDfy();
        }

        [TestCase(null)]
        [TestCase("")]
        public void Adding_contact_with_no_title_and_no_handles_should_not_yeild_a_contact(string title)
        {
            var contact = GenerateContact("PrimaryAccount", title, "Stub-John@doe.com");
            new UserContactUpdateSingleContactAggregateScenario()
               .Given(s => s.Given_a_UserContacts_instance())
               .When(s => s.When_a_ContactSummary_is_added(contact))
               .Then(s => s.Then_Snapshot_is_empty())
               .BDDfy();
        }

        [Test]
        public void Merging_multiple_contacts()
        {
            var accId = "ABC";
            var barx = new StubContactSummary
            {
                Provider = "Google",
                AccountId = accId,
                Title = "Campbell, Erynne",
                ProviderId = "http://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/base/37cc52308c94e30f",
                Handles =
                {
                    new ContactEmailAddress("erynne.campbell@barclays.com", "work"),
                    new ContactEmailAddress("Erynne.Campbell@barclayscapital.com", "work"),
                }
            };
            var old = new StubContactSummary
            {
                Provider = "Google",
                AccountId = accId,
                Title = "Erynne Campbell",
                ProviderId = "http://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/base/aa",
                Handles =
                {
                    new ContactEmailAddress("Erynne.Campbell@googlemail.com", "Obsolete"),
                    new ContactPhoneNumber("+61417910632", "mobile"),
                    new ContactPhoneNumber("+447554257819", "mobile"),
                }
            };
            var current = new StubContactSummary
            {
                Provider = "Google",
                AccountId = accId,
                Title = "Erynne Campbell",
                ProviderId = "http://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/base/78a2b08a3e7700",
                Handles =
                {
                    new ContactEmailAddress("Erynne.Campbell@gmail.com", "Obsolete"),                    
                }
            };

            var expected = new ContactAggregateUpdate()
            {
                //Id = ??
                Version = 3,
                NewTitle = "Erynne Campbell",
                IsDeleted = false,
                AddedProviders = new ContactProviderSummary[]
                {
                    new ContactProviderSummary{
                        AccountId = accId,
                        ContactId = "http://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/base/37cc52308c94e30f",
                        ProviderName = "Google"
     
                    },
                    new ContactProviderSummary{
                        AccountId = accId,
                        ContactId =  "http://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/base/aa",
                        ProviderName = "Google"
                    },
                    new ContactProviderSummary{
                        AccountId = accId,
                        ContactId =  "http://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/base/78a2b08a3e7700",
                        ProviderName = "Google"
                    }
               },
               AddedHandles = new ContactHandle[]
               {
                   new ContactEmailAddress("erynne.campbell@barclays.com", "work"),
                   new ContactEmailAddress("Erynne.Campbell@barclayscapital.com", "work"),
                   new ContactEmailAddress("Erynne.Campbell@googlemail.com", "Obsolete"),
                   new ContactPhoneNumber("+61417910632", "mobile"),
                   new ContactPhoneNumber("+447554257819", "mobile"),
                   new ContactEmailAddress("Erynne.Campbell@gmail.com", "Obsolete"),                    
               }
            };

            new UserContactUpdateSingleContactAggregateScenario()
               .Given(s => s.Given_a_UserContacts_instance())
               .When(s => s.When_a_ContactSummary_is_added(barx))
               .When(s => s.When_a_ContactSummary_is_added(old))
               .When(s => s.When_a_ContactSummary_is_added(current))
               .Then(s => s.Then_Snapshot_has_only(expected))
               .BDDfy();
        }


        private static StubContactSummary GenerateContact(string accountId, string title, string providerId, string email = null)
        {
            var contact = new StubContactSummary
            {
                Provider = "StubProvider",
                AccountId = accountId,
                Title = title,
                ProviderId = providerId
            };
            if (email != null)
                contact.Handles.Add(new ContactEmailAddress(email, "Home"));

            return contact;
        }

        public class UserContactUpdateSingleContactAggregateScenario
        {
            private UserContacts _userContacts;

            public void Given_a_UserContacts_instance()
            {
                _userContacts = new UserContacts(Guid.NewGuid());
                _userContacts.TrackChanges();
            }

            public void When_a_ContactSummary_is_added(IAccountContactSummary contactSummary)
            {
                _userContacts.Add(contactSummary);
            }

            public void Then_Snapshot_includes_contact(Func<ContactAggregateUpdate, bool> matchingContact)
            {
                var snapshot = _userContacts.GetChangesSnapshot();
                Assert.NotNull(snapshot.SingleOrDefault(matchingContact));
            }
            public void Then_Snapshot_has_only(ContactAggregateUpdate expected)
            {
                var snapshot = _userContacts.GetChangesSnapshot();
                var actual = snapshot.Single();

                //Assert.AreEqual(expected.Id);
                Assert.AreEqual(expected.Version, actual.Version);
                Assert.AreEqual(expected.NewTitle, actual.NewTitle);
                CollectionAssert.AreEqual(expected.AddedAvatars, actual.AddedAvatars);
                CollectionAssert.AreEqual(expected.RemovedAvatars, actual.RemovedAvatars);

                CollectionAssert.AreEqual(expected.AddedTags, actual.AddedTags);
                CollectionAssert.AreEqual(expected.RemovedTags, actual.RemovedTags);

                CollectionAssert.AreEqual(expected.AddedProviders, actual.AddedProviders, ContactProviderSummaryComparer.Instance);
                CollectionAssert.AreEqual(expected.RemovedProviders, actual.RemovedProviders, ContactProviderSummaryComparer.Instance);

                CollectionAssert.AreEqual(expected.AddedHandles, actual.AddedHandles);
                CollectionAssert.AreEqual(expected.RemovedHandles, actual.RemovedHandles);
            }
            public void Then_Snapshot_is_empty()
            {
                var snapshot = _userContacts.GetChangesSnapshot();

                CollectionAssert.IsEmpty(snapshot);
            }
        }

        public class UserContactRemovedScenario
        {
            private UserContacts _userContacts;

            public void Given_a_populated_UserContacts_instance(params IAccountContactSummary[] contacts)
            {
                _userContacts = new UserContacts(Guid.NewGuid());
                using (_userContacts.TrackChanges())
                {
                    foreach (var contact in contacts)
                    {
                        _userContacts.Add(contact);
                    }
                    _userContacts.GetChangesSnapshot();
                    _userContacts.CommitChanges();
                }
            }

            public void When_a_ContactSummary_is_removed(IAccountContactSummary contactSummary)
            {
                if (!contactSummary.IsDeleted) throw new ArgumentException("contactSummary must be flagged as deleted", "contactSummary");
                _userContacts.TrackChanges();
                _userContacts.Add(contactSummary);
            }

            public void Then_Snapshot_has_only(ContactAggregateUpdate expected)
            {
                var snapshot = _userContacts.GetChangesSnapshot();
                var actual = snapshot.Single();

                //Assert.AreEqual(expected.Id);
                Assert.AreEqual(expected.Version, actual.Version);
                Assert.AreEqual(expected.NewTitle, actual.NewTitle);
                CollectionAssert.AreEqual(expected.AddedAvatars, actual.AddedAvatars);
                CollectionAssert.AreEqual(expected.RemovedAvatars, actual.RemovedAvatars);

                CollectionAssert.AreEqual(expected.AddedTags, actual.AddedTags);
                CollectionAssert.AreEqual(expected.RemovedTags, actual.RemovedTags);

                CollectionAssert.AreEqual(expected.AddedProviders, actual.AddedProviders, ContactProviderSummaryComparer.Instance);
                CollectionAssert.AreEqual(expected.RemovedProviders, actual.RemovedProviders, ContactProviderSummaryComparer.Instance);

                CollectionAssert.AreEqual(expected.AddedHandles, actual.AddedHandles);
                CollectionAssert.AreEqual(expected.RemovedHandles, actual.RemovedHandles);
            }
        }
    }
}
