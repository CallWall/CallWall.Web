using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Providers.Contacts;
using NSubstitute;
using NUnit.Framework;
using TestStack.BDDfy;

namespace CallWall.Web.GoogleProvider.Tests
{
    [TestFixture]
    public class GoogleContactProfileTranslatorFixture
    {
        //TODO: Write tests for the GoogleContactProfileTranslator taking snippets from SampleContactRequest_response_large.xml

        [Test]
        public void Should_translate_Xcontact_with_gdDeleted_element_as_IsDeleted_AccountContactSummary()
        {
            var providerId = "StubProvider-Contact1";
            var account = Substitute.For<IAccount>();
            account.AccountId.Returns("MyAccountId");

            new DeletedContactScenario()
                .Given(s => s.Given_a_response_with_gdDeleted_element(providerId))
                .When(s => s.When_translating_with_("someAccessToken", account))
                .Then(s => s.Then_result_batch_contains_deleted_record(providerId))
                .BDDfy();
        }

        [Test]
        public void Should_translate_ContactHandles()
        {
            var account = Substitute.For<IAccount>();
            account.AccountId.Returns("MyAccountId");

            new NewContactScenario()
                .Given(s => s.Given_a_payload_with_new_Entry())
                .When(s => s.When_translating_with_(account))
                .Then(s => s.Then_result_batch_contains_phone_record())
                .And(s => s.Then_result_batch_contains_emailrecord())
                .BDDfy();
        }

        public class NewContactScenario
        {
            private readonly GoogleContactProfileTranslator _sut;
            private string _responsePayload;
            private BatchOperationPage<IAccountContactSummary> _batch;

            public NewContactScenario()
            {
                _sut = new GoogleContactProfileTranslator();
            }

            public void Given_a_payload_with_new_Entry()
            {
                _responsePayload = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<feed gd:etag=""&quot;QHg6eDVSLyt7I2A9XRVTE0gKTwI.&quot;"" xmlns=""http://www.w3.org/2005/Atom"" xmlns:batch=""http://schemas.google.com/gdata/batch"" xmlns:gContact=""http://schemas.google.com/contact/2008"" xmlns:gd=""http://schemas.google.com/g/2005"" xmlns:openSearch=""http://a9.com/-/spec/opensearch/1.1/"">
 <id>lee.ryan.campbell@gmail.com</id>
 <updated>2015-04-29T23:41:01.610Z</updated>
 <category scheme=""http://schemas.google.com/g/2005#kind"" term=""http://schemas.google.com/contact/2008#contact""/>
 <title>Lee Campbell's Contacts</title>
 <link rel=""alternate"" type=""text/html"" href=""https://www.google.com/""/>
 <link rel=""http://schemas.google.com/g/2005#feed"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full""/>
 <link rel=""http://schemas.google.com/g/2005#post"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full""/>
 <link rel=""http://schemas.google.com/g/2005#batch"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full/batch""/>
 <link rel=""self"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full?max-results=1000&amp;showdeleted=true&amp;start-index=1""/>
 <link rel=""next"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full?max-results=1000&amp;showdeleted=true&amp;start-index=1001""/>
 <author>
  <name>Lee Campbell</name>
  <email>lee.ryan.campbell@gmail.com</email>
 </author>
 <generator version=""1.0"" uri=""http://www.google.com/m8/feeds"">Contacts</generator>
 <openSearch:totalResults>1218</openSearch:totalResults>
 <openSearch:startIndex>1</openSearch:startIndex>
 <openSearch:itemsPerPage>1000</openSearch:itemsPerPage>
 <entry gd:etag=""&quot;RHwzcTVSLyt7I2A9XRdUGUgDRgE.&quot;"">
  <id>http://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/base/20</id>
  <updated>2014-12-07T13:32:35.289Z</updated>
  <app:edited xmlns:app=""http://www.w3.org/2007/app"">2014-12-07T13:32:35.289Z</app:edited>
  <category scheme=""http://schemas.google.com/g/2005#kind"" term=""http://schemas.google.com/contact/2008#contact""/>
  <title>Mark D Sievers</title>
  <content>4 Falstone Lane
Browns Bay
Auckland 0630
NZ&lt;HTCData&gt;&lt;Twitter&gt;id:19230744/friendof:18681531&lt;/Twitter&gt;&lt;/HTCData&gt;</content>
  <link rel=""http://schemas.google.com/contacts/2008/rel#photo"" type=""image/*"" href=""https://www.google.com/m8/feeds/photos/media/lee.ryan.campbell%40gmail.com/20"" gd:etag=""&quot;czhoH31DWit7I2BkB3wJFjdWWnA-D30fQCg.&quot;""/>
  <link rel=""self"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full/20""/>
  <link rel=""edit"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full/20""/>
  <gd:name>
   <gd:fullName>Mark D Sievers</gd:fullName>
   <gd:givenName>Mark</gd:givenName>
   <gd:additionalName>D</gd:additionalName>
   <gd:familyName>Sievers</gd:familyName>
  </gd:name>
  <gd:email rel=""http://schemas.google.com/g/2005#other"" address=""mark.sievers@gmail.com"" primary=""true""/>
  <gd:phoneNumber rel=""http://schemas.google.com/g/2005#mobile"" primary=""true"" uri=""tel:+64-210-250-5196"">+64 21 025 05196</gd:phoneNumber>
  <gContact:relation rel=""spouse"">Kelly WIlls</gContact:relation>
  <gContact:website href=""http://www.google.com/profiles/107329049070789932728"" rel=""profile""/>
  <gContact:groupMembershipInfo deleted=""false"" href=""http://www.google.com/m8/feeds/groups/lee.ryan.campbell%40gmail.com/base/6""/>
 </entry>
</feed>";
            }

            public void When_translating_with_(IAccount account)
            {
                _batch = _sut.Translate(_responsePayload, "accessToken", account, new Dictionary<string, string> { { "http://www.google.com/m8/feeds/groups/lee.ryan.campbell%40gmail.com/base/6", "MyGroup" } });
            }

            public void Then_result_batch_contains_phone_record()
            {
                var actual = _batch.Items.Single();

                Assert.AreEqual("mobile", actual.Handles.Single(h => h.HandleType==ContactHandleTypes.Phone).Qualifier);
                Assert.AreEqual("+64-210-250-5196", actual.Handles.Single(h => h.HandleType==ContactHandleTypes.Phone).Handle);
            }

            public void Then_result_batch_contains_emailrecord()
            {
                var actual = _batch.Items.Single();

                Assert.AreEqual("mark.sievers@gmail.com", actual.Handles.Single(h => h.HandleType == ContactHandleTypes.Email).Handle);
            }
        }

        public class DeletedContactScenario
        {
            private readonly GoogleContactProfileTranslator _sut;
            private string _responsePayload;
            private BatchOperationPage<IAccountContactSummary> _batch;
            private IAccount _account;

            public DeletedContactScenario()
            {
                _sut = new GoogleContactProfileTranslator();
            }


            public void Given_a_response_with_gdDeleted_element(string providerId)
            {
                _responsePayload = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<feed gd:etag=""&quot;QHw8ezVSLyt7I2A9XRdRGEULRAE.&quot;"" xmlns=""http://www.w3.org/2005/Atom"" xmlns:batch=""http://schemas.google.com/gdata/batch"" xmlns:gContact=""http://schemas.google.com/contact/2008"" xmlns:gd=""http://schemas.google.com/g/2005"" xmlns:openSearch=""http://a9.com/-/spec/opensearch/1.1/"">
 <id>lee.ryan.campbell@gmail.com</id>
 <updated>2014-10-09T19:02:31.273Z</updated>
 <category scheme=""http://schemas.google.com/g/2005#kind"" term=""http://schemas.google.com/contact/2008#contact""/>
 <title>Lee Campbell's Contacts</title>
 <link rel=""alternate"" type=""text/html"" href=""https://www.google.com/""/>
 <link rel=""http://schemas.google.com/g/2005#feed"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full""/>
 <link rel=""http://schemas.google.com/g/2005#post"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full""/>
 <link rel=""http://schemas.google.com/g/2005#batch"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full/batch""/>
 <link rel=""self"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full?max-results=1000&amp;showdeleted=true&amp;start-index=1""/>
 <link rel=""next"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full?max-results=1000&amp;showdeleted=true&amp;start-index=1001""/>
 <author>
  <name>Lee Campbell</name>
  <email>lee.ryan.campbell@gmail.com</email>
 </author>
 <generator version=""1.0"" uri=""http://www.google.com/m8/feeds"">Contacts</generator>
 <openSearch:totalResults>1214</openSearch:totalResults>
 <openSearch:startIndex>1</openSearch:startIndex>
 <openSearch:itemsPerPage>1000</openSearch:itemsPerPage>
 <entry gd:etag=""&quot;SXs8fDVSLit7I2A9XRdSF08OTgA.&quot;"">
  <id>" + providerId + @"</id>
  <updated>2014-09-26T09:49:08.574Z</updated>
  <app:edited xmlns:app=""http://www.w3.org/2007/app"">2014-09-26T09:49:08.574Z</app:edited>
  <category scheme=""http://schemas.google.com/g/2005#kind"" term=""http://schemas.google.com/contact/2008#contact""/>
  <title/>
  <link rel=""http://schemas.google.com/contacts/2008/rel#photo"" type=""image/*"" href=""https://www.google.com/m8/feeds/photos/media/lee.ryan.campbell%40gmail.com/3eab44ab0b9b9db9""/>
  <link rel=""self"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full/3eab44ab0b9b9db9""/>
  <link rel=""edit"" type=""application/atom+xml"" href=""https://www.google.com/m8/feeds/contacts/lee.ryan.campbell%40gmail.com/full/3eab44ab0b9b9db9""/>
  <gd:deleted/>
  <gContact:groupMembershipInfo deleted=""true"" href=""http://www.google.com/m8/feeds/groups/lee.ryan.campbell%40gmail.com/base/6""/>
 </entry>
</feed>";
            }

            public void When_translating_with_(string accessToken, IAccount account)
            {
                _account = account;
                _batch = _sut.Translate(_responsePayload, "accessToken", account, new Dictionary<string, string> { { "http://www.google.com/m8/feeds/groups/lee.ryan.campbell%40gmail.com/base/6","MyGroup" } });
            }

            public void Then_result_batch_contains_deleted_record(string providerId)
            {
                var actual = _batch.Items.Single();

                Assert.AreEqual(true, actual.IsDeleted);
                Assert.AreEqual("Google", actual.Provider);
                Assert.AreEqual(providerId, actual.ProviderId);
                Assert.AreEqual(_account.AccountId, actual.AccountId);
            }
        }

        [Test]
        public void Create_Json_from_GoogleResponse()
        {
            var account = Substitute.For<IAccount>();
            account.AccountId.Returns("MyAccountId");
            var httpReponse = File.ReadAllText(@"SampleContactRequest_response_large.xml");
            var groupsHttpReponse = File.ReadAllText(@"ContactGroups_response.xml");

            var translator = new GoogleContactProfileTranslator();
            var groups = translator.ToGroupDictionary(groupsHttpReponse);
            var results = translator.Translate(httpReponse, "junkAccessToken", account, groups);

            var titles = results.Items
                .Where(summary => !summary.IsDeleted
                    && !string.IsNullOrWhiteSpace(summary.Title)
                    && summary.Title.ToLowerInvariant().Contains("erynne"))
                .Select(summary => summary.Title);
            foreach (var title in titles)
            {
                Console.WriteLine(title);
            }


            var count = results.Items.Count(
                summary =>
                    !summary.IsDeleted
                    && !string.IsNullOrWhiteSpace(summary.Title)
                    && summary.Title.ToLowerInvariant().Contains("erynne"));

            Assert.AreEqual(3, count);
        }
    }
}
