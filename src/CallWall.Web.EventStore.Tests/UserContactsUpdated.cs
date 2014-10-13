using System;
using System.Collections;
using System.Collections.Generic;
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
    public class UserContactsUpdated
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

        //[Test]
        //public void RemovingContact()
        //{
        //    throw new NotImplementedException();
        //}

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
                AddedAvatars = new string[0],
                AddedTags = new string[0],
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
        }

        public sealed class ContactProviderSummaryComparer : IComparer, IComparer<IContactProviderSummary>
        {
            public static readonly ContactProviderSummaryComparer Instance = new ContactProviderSummaryComparer();
            public int Compare(object x, object y)
            {
                var lhs = x as IContactProviderSummary;
                var rhs = y as IContactProviderSummary;
                return Compare(lhs, rhs);
            }

            public int Compare(IContactProviderSummary x, IContactProviderSummary y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                var providerSort = String.CompareOrdinal(x.ProviderName, y.ProviderName);
                if (providerSort != 0) return providerSort;

                var accountSort = String.CompareOrdinal(x.AccountId, y.AccountId);
                if (accountSort != 0) return accountSort;

                return String.CompareOrdinal(x.ContactId, y.ContactId);
            }
        }
    }
}
