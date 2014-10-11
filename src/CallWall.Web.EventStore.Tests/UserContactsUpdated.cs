using System;
using System.Linq;
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
        private Guid _userId;

        [SetUp]
        public void SetUp()
        {
            _userId = Guid.NewGuid();
        }

        [Test]
        public void AddingNewContact()
        {
            var contact = GenerateContact("PrimaryAccount", "John Doe", "Stub-John@doe.com");
            new UserContactUpdateSingleContactAggregateScenario()
               .Given(s => s.Given_a_UserContacts_instance())
               .When(s => s.When_a_ContactSummary_is_added(contact))
               .Then(s => s.Then_Snapshot_includes_contact(c=>c.NewTitle==contact.Title 
                   && c.AddedProviders.Single().ProviderName==contact.Provider
                   && c.AddedProviders.Single().AccountId==contact.AccountId
                   && c.AddedProviders.Single().ContactId==contact.ProviderId
                   ))
               .BDDfy();
        }

        


        //[Test]
        //public void UpdatingContact()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test]
        //public void RemovingContact()
        //{
        //    throw new NotImplementedException();
        //}


        //[Test]
        //public void UpdatingContactAggregateWithMatchingContact()
        //{
        //    throw new NotImplementedException();
        //}


        private IAccountContactSummary GenerateContact(string accountId, string title, string providerId)
        {
            return new StubContactSummary
            {
                Provider = "StubProvider",
                AccountId = accountId,
                Title = title,
                ProviderId = providerId
            };
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

            public void Then_Snapshot_includes_contact(Func<ContactAggregateUpdate,bool> matchingContact)
            {
                var snapshot = _userContacts.GetChangesSnapshot();
                Assert.NotNull(snapshot.SingleOrDefault(matchingContact));
            }
        }
    }
}
