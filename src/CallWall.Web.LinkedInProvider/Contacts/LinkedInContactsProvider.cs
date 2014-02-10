using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;
using CallWall.Web.Providers;
using Newtonsoft.Json;

namespace CallWall.Web.LinkedInProvider.Contacts
{
    public class LinkedInContactsProvider : IContactsProvider
    {
        public IObservable<IFeed<IContactSummary>> GetContactsFeed(IEnumerable<ISession> sessions, IEnumerable<IClientLastUpdated> lastUpdatedDetails)
        {
            var session = sessions.SingleOrDefault(s => s.Provider == "LinkedIn");
            if (session == null)
                return Observable.Empty<ContactFeed>();
            var lastUpdated = lastUpdatedDetails.Where(s => s.Provider == "LinkedIn").Select(s => s.LastUpdated).FirstOrDefault();
            return Observable.Create<ContactFeed>(o =>
            {
                try
                {
                    var feed = new ContactFeed(session, lastUpdated);
                    return Observable.Return(feed).Subscribe(o);
                }
                catch (Exception ex)
                {
                    return Observable.Throw<ContactFeed>(ex).Subscribe(o);
                }
            });
        }

        public IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys)
        {
            //TODO: Implement LinkedIn GetContactDetails
            return Observable.Empty<IContactProfile>();
        }

        private sealed class ContactFeed : IFeed<IContactSummary>
        {
            private readonly int _totalResults;
            private readonly IObservable<IContactSummary> _values;
            private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

            public ContactFeed(ISession session, DateTime lastUpdated)
            {
                //TODO - this shouldnt be in a ctor - but it doesnt need to complexity of the Google provider - review with Lee - RC
                var client = new HttpClient();
                var requestUriBuilder = new UriBuilder("https://api.linkedin.com/v1/people/~/connections");
                requestUriBuilder.AddQuery("oauth2_access_token", HttpUtility.UrlEncode(session.AccessToken));
                if (lastUpdated != default(DateTime))
                {
                    var lastModifedAsUnixTimestamp = (lastUpdated - UnixEpoch).TotalMilliseconds.ToString(CultureInfo.InvariantCulture)
                                                                             .Split('.')
                                                                             .First();
                    requestUriBuilder.AddQuery("modified", "updated")
                                     .AddQuery("modified-since", lastModifedAsUnixTimestamp);
                }
                var request = new HttpRequestMessage(HttpMethod.Get, requestUriBuilder.Uri);

                request.Headers.Add("x-li-format", "json");

                //TODO: Add error handling (not just exceptions but also non 200 responses -LC
                var response = client.SendAsync(request);
                var contactResponse = response.ContinueWith(r => r.Result.Content.ReadAsStringAsync()).Unwrap().Result;

                var contacts = JsonConvert.DeserializeObject<ContactsResponse>(contactResponse);
                _totalResults = contacts.Total;
                if (_totalResults > 0 && contacts.Contacts != null)
                    _values = contacts.Contacts.Select(TranslateToContactSummary).ToObservable();
                else
                    _values = Observable.Empty<IContactSummary>();
            }

            public int TotalResults { get { return _totalResults; } }

            public IObservable<IContactSummary> Values { get { return _values; } }

            private static IContactSummary TranslateToContactSummary(Contact c)
            {
                return new ContactSummary(c.Id, c.FirstName, c.LastName, c.PictureUrl, new []{c.Industry, c.Headline});
            }
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
