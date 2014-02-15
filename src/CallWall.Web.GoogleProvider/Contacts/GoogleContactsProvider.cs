using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CallWall.Web.Contracts.Contact;
using CallWall.Web.GoogleProvider.Providers.Contacts;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider.Contacts
{
    internal sealed class GoogleContactsProvider : IContactsProvider
    {
        public IObservable<IFeed<IContactSummary>> GetContactsFeed(IEnumerable<ISession> sessions, IEnumerable<IClientLastUpdated> lastUpdatedDetails)
        {
            var session = sessions.SingleOrDefault(s => s.Provider == "Google");
            if (session == null)
                return Observable.Empty<ContactFeed>();
            var lastUpdated = lastUpdatedDetails.Where(s => s.Provider == "Google").Select(s => s.LastUpdated).FirstOrDefault();
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
            //TODO: Implement Google GetContactDetails
            return Observable.Empty<IContactProfile>();
        }

        private sealed class ContactFeed : IFeed<IContactSummary>
        {
            private readonly int _totalResults;
            private readonly IObservable<IContactSummary> _values;

            public ContactFeed(ISession session, DateTime lastUpdated)
            {
                var batchPage = GetContactPage(session, 1, lastUpdated);
                _totalResults = batchPage.TotalResults;
                _values = GenerateValues(session, lastUpdated, batchPage);
            }

            public int TotalResults { get { return _totalResults; } }

            public IObservable<IContactSummary> Values { get { return _values; } }

            private IObservable<IContactSummary> GenerateValues(ISession session, DateTime lastUpdated, BatchOperationPage<IContactSummary> batchPage)
            {
                return Observable.Create<IContactSummary>(o =>
                {
                    var pages = GetPages(session, lastUpdated, batchPage);
                    var query = from page in pages
                                from contact in page.Items
                                select contact;
                    return query.Subscribe(o);
                });
            }

            private static IEnumerable<BatchOperationPage<IContactSummary>> GetPages(ISession session, DateTime lastUpdated, BatchOperationPage<IContactSummary> batchPage)
            {
                yield return batchPage;
                while (batchPage.NextPageStartIndex > 0)
                {
                    //HACK:Google doesn't like being DOS'ed.
                    //Thread.Sleep(1000);  //HACK:Google doesn't like being DOS'ed.
                    Thread.Sleep(500);
                    //Thread.Sleep(250);  
                    batchPage = GetContactPage(session, batchPage.NextPageStartIndex, lastUpdated);
                    yield return batchPage;
                }
            }

            private static BatchOperationPage<IContactSummary> GetContactPage(ISession session, int startIndex, DateTime lastUpdated)
            {
                var client = new HttpClient();

                var requestUriBuilder = new UriBuilder("https://www.google.com/m8/feeds/contacts/default/full");
                requestUriBuilder.AddQuery("access_token", HttpUtility.UrlEncode(session.AccessToken))
                                 .AddQuery("start-index", startIndex.ToString(CultureInfo.InvariantCulture));

                if (lastUpdated != default(DateTime))
                {
                    var formattedDate = lastUpdated.ToString("yyyy-MM-ddT00:00:00");
                    requestUriBuilder.AddQuery("updated-min", formattedDate);
                }
                var request = new HttpRequestMessage(HttpMethod.Get, requestUriBuilder.Uri);
                request.Headers.Add("GData-Version", "3.0");

                //TODO: Add error handling (not just exceptions but also non 200 responses -LC
                try
                {
                    var response = client.SendAsync(request);
                    var contactResponse = response.ContinueWith(r =>
                        {
                            r.Result.EnsureSuccessStatusCode();
                            return r.Result.Content.ReadAsStringAsync();
                        }).Unwrap().Result;

                    var translator = new GoogleContactProfileTranslator();//TODO - ioc??
                    var contacts = translator.TranslateToPagedContactSummaries(contactResponse, session.AccessToken);

                    return contacts;
                }
                catch (Exception exception)
                {
                    //TODO logging? do we want a logging factory?
                    return BatchOperationPage<IContactSummary>.Empty();
                }
            }
        }
    }
}
