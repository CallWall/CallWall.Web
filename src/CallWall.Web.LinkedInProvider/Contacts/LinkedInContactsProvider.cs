using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace CallWall.Web.LinkedInProvider.Contacts
{
    public class LinkedInContactsProvider : IContactsProvider
    {
        public IObservable<IFeed<IContactSummary>> GetContactsFeed(IEnumerable<ISession> sessions)
        {
            var session = sessions.SingleOrDefault(s => s.Provider == "LinkedIn");
            if (session == null)
                return Observable.Empty<ContactFeed>();
            return Observable.Create<ContactFeed>(o =>
            {
                try
                {
                    var feed = new ContactFeed(session);
                    return Observable.Return(feed).Subscribe(o);
                }
                catch (Exception ex)
                {
                    return Observable.Throw<ContactFeed>(ex).Subscribe(o);
                }
            });
        }

        private sealed class ContactFeed : IFeed<IContactSummary>
        {
            private readonly int _totalResults;
            private readonly IObservable<IContactSummary> _values;

            public ContactFeed(ISession session)
            {
                //TODO - this shouldnt be in a ctor - but it doesnt need to complexity of the Google provider - review with Lee - RC
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v1/people/~/connections?oauth2_access_token=" + HttpUtility.UrlEncode(session.AccessToken));
                request.Headers.Add("x-li-format", "json");

                //TODO: Add error handling (not just exceptions but also non 200 responses -LC
                var response = client.SendAsync(request);
                var contactResponse = response.ContinueWith(r => r.Result.Content.ReadAsStringAsync()).Unwrap().Result;

                var contacts = JsonConvert.DeserializeObject<ContactsResponse>(contactResponse);
                _totalResults = contacts.Total;
                _values = contacts.Contacts.Select(TranslateToContactSummary).ToObservable();
            }

            public int TotalResults { get { return _totalResults; } }

            public IObservable<IContactSummary> Values { get { return _values; } }
            
            private static IContactSummary TranslateToContactSummary(Contact c)
            {
                return new ContactSummary(c.FirstName, c.LastName, c.PictureUrl, Enumerable.Empty<string>());
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
