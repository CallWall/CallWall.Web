using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Web;
using CallWall.Web.Domain;
using CallWall.Web.Providers;
using Newtonsoft.Json;

namespace CallWall.Web.LinkedInProvider.Contacts
{
    public class LinkedInAccountContactProvider : IAccountContactProvider
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        public string Provider { get { return "LinkedIn"; } }

        public IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            if (account.Provider != Provider)
                return Observable.Empty<IAccountContactSummary>();

            return Observable.Create<IAccountContactSummary>(
                async (o, ct) =>
                {

                    try
                    {
                        var client = new HttpClient();
                        var request = CreateContactSummaryListRequest(account, lastUpdated);

                        //TODO: Add error handling (not just exceptions but also non 200 responses -LC
                        var response = await client.SendAsync(request, ct);
                        response.EnsureSuccessStatusCode();
                        var contactResponse = await response.Content.ReadAsStringAsync();

                        var contacts = JsonConvert.DeserializeObject<ContactsResponse>(contactResponse);
                        if (contacts.Total <= 0 || contacts.Contacts == null)
                        {
                            return Observable.Empty<IAccountContactSummary>().Subscribe(o);
                        }
                        return contacts.Contacts
                            .Select(c => TranslateToContactSummary(account.AccountId, c))
                            .ToObservable()
                            .Subscribe(o);
                    }
                    catch (Exception e)
                    {
                        return Observable.Throw<IAccountContactSummary>(e).Subscribe(o);
                    }
                });

        }

        public IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys)
        {
            //TODO: Implement LinkedIn GetContactDetails
            return Observable.Empty<IContactProfile>();
        }

        private static HttpRequestMessage CreateContactSummaryListRequest(IAccount account, DateTime lastUpdated)
        {
            var requestUriBuilder = new UriBuilder("https://api.linkedin.com/v1/people/~/connections");

            requestUriBuilder.AddQuery("oauth2_access_token", HttpUtility.UrlEncode(account.CurrentSession.AccessToken));
            if (lastUpdated != default(DateTime))
            {
                var lastModifedAsUnixTimestamp =
                    (lastUpdated - UnixEpoch).TotalMilliseconds.ToString(CultureInfo.InvariantCulture)
                        .Split('.')
                        .First();
                requestUriBuilder.AddQuery("modified", "updated")
                    .AddQuery("modified-since", lastModifedAsUnixTimestamp);
            }
            var request = new HttpRequestMessage(HttpMethod.Get, requestUriBuilder.Uri);

            request.Headers.Add("x-li-format", "json");
            return request;
        }

        private static IAccountContactSummary TranslateToContactSummary(string accountId, Contact c)
        {
            return new ContactSummary(accountId, c.Id, c.FirstName, c.LastName, c.PictureUrl, new[] { c.Industry, c.Headline });
        }
    }

    #region JsModel
    public class ContactsResponse
    {
        [JsonProperty("_total")]
        public int Total { get; set; }
        [JsonProperty("values")]
        public Contact[] Contacts { get; set; }
    }

    public class Contact
    {
        public string FirstName { get; set; }
        public string Headline { get; set; }
        public string Id { get; set; }
        public string Industry { get; set; }
        public string LastName { get; set; }
        public Location Location { get; set; }
        public string PictureUrl { get; set; }
    }

    public class Location
    {
        public Country Country { get; set; }
        public string Name { get; set; }
    }

    public class Country
    {
        public string Code { get; set; }
    }
    #endregion
}
