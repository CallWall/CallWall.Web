using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using CallWall.Web.Models;

namespace CallWall.Web.Providers.Google
{
    public class GoogleContactsProvider
    {
        public IEnumerable<IContactSummary> GetContacts(ISession session)
        {
            var contacts = new List<IContactSummary>();
            var batchPage = GetContactPage(session, 1);
            contacts.AddRange(batchPage.Items);
            while (batchPage.NextPageStartIndex > 0)
            {
                batchPage = GetContactPage(session, batchPage.NextPageStartIndex);
                contacts.AddRange(batchPage.Items);
            }

            return contacts;
        }

        private BatchOperationPage<IContactSummary> GetContactPage(ISession session, int startIndex)
        {
            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com/m8/feeds/contacts/default/full?access_token=" + HttpUtility.UrlEncode(session.AccessToken) + "&start-index=" + startIndex);
            request.Headers.Add("GData-Version", "3.0");

            var response = client.SendAsync(request);
            var result = response.Result;
            if(!result.IsSuccessStatusCode)
                throw new Exception();

            var accessTokenResponse = result.Content.ReadAsStringAsync();
            var output = accessTokenResponse.Result;

            var translator = new GoogleContactProfileTranslator();
            var contacts = translator.Translate(output, session.AccessToken);

            return contacts;
        }
    }
}