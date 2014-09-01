using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider.Contacts
{
    internal sealed class GoogleAccountContactProvider : IAccountContactProvider
    {
        public string Provider { get { return "Google"; } }

        public IObservable<IFeed<IAccountContactSummary>> GetContactsFeed(IAccountData account, DateTime lastUpdated)
        {
            if (account.Provider != Provider)
                return Observable.Empty<ContactFeed>();
            return Observable.Create<ContactFeed>(o =>
              {
                  try
                  {
                      var feed = new ContactFeed(account, lastUpdated);
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

        private sealed class ContactFeed : IFeed<IAccountContactSummary>
        {
            private readonly int _totalResults;
            private readonly IObservable<IAccountContactSummary> _values;

            public ContactFeed(IAccountData account, DateTime lastUpdated)
            {
                var batchPage = GetContactPage(account, 1, lastUpdated);
                _totalResults = batchPage.TotalResults;
                _values = GenerateValues(account, lastUpdated, batchPage);
            }

            public int TotalResults { get { return _totalResults; } }

            public IObservable<IAccountContactSummary> Values { get { return _values; } }

            private IObservable<IAccountContactSummary> GenerateValues(IAccountData account, DateTime lastUpdated, BatchOperationPage<IAccountContactSummary> batchPage)
            {
                return Observable.Create<IAccountContactSummary>(o =>
                {
                    var pages = GetPages(account, lastUpdated, batchPage);
                    var query = from page in pages
                                from contact in page.Items
                                select contact;
                    return query.Subscribe(o);
                });
            }

            private static IEnumerable<BatchOperationPage<IAccountContactSummary>> GetPages(IAccountData account, DateTime lastUpdated, BatchOperationPage<IAccountContactSummary> batchPage)
            {
                yield return batchPage;
                while (batchPage.NextPageStartIndex > 0)
                {
                    //HACK:Google doesn't like being DOS'ed.
                    //Thread.Sleep(1000);  //HACK:Google doesn't like being DOS'ed.
                    Thread.Sleep(500);
                    //Thread.Sleep(250);  
                    batchPage = GetContactPage(account, batchPage.NextPageStartIndex, lastUpdated);
                    yield return batchPage;
                }
            }

            private static BatchOperationPage<IAccountContactSummary> GetContactPage(IAccountData account, int startIndex, DateTime lastUpdated)
            {
                var client = new HttpClient();

                var requestUriBuilder = new UriBuilder("https://www.google.com/m8/feeds/contacts/default/full");
                requestUriBuilder.AddQuery("access_token", HttpUtility.UrlEncode(account.CurrentSession.AccessToken))
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

                    var translator = new GoogleContactProfileTranslator();
                    var contacts = translator.Translate(contactResponse, account.CurrentSession.AccessToken, account);

                    return contacts;
                }
                catch (Exception exception)
                {
                    //TODO logging? do we want a logging factory?
                    return BatchOperationPage<IAccountContactSummary>.Empty();
                }
            }
        }
    }
}
