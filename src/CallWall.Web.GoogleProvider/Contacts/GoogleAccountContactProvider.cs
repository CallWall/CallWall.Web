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
        private ILogger _logger;

        public GoogleAccountContactProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }
        public string Provider { get { return "Google"; } }

        public IObservable<IFeed<IAccountContactSummary>> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {

            if (account.Provider != Provider)
                return Observable.Empty<ContactFeed>();

            
            return Observable.Create<ContactFeed>(o =>
              {
                  try
                  {
                      var feed = new ContactFeed(account, lastUpdated, _logger);
                      return Observable.Return(feed).Subscribe(o);
                  }
                  catch (Exception ex)
                  {
                      return Observable.Throw<ContactFeed>(ex).Subscribe(o);
                  }
              })
              .Log(_logger, "GetContactsFeed(" + account.AccountId + ")");
        }

        public IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys)
        {
            //TODO: Implement Google GetContactDetails
            return Observable.Empty<IContactProfile>();
        }

        private sealed class ContactFeed : IFeed<IAccountContactSummary>
        {
            private readonly ILogger _logger;
            private readonly int _totalResults;
            private readonly IObservable<IAccountContactSummary> _values;

            public ContactFeed(IAccount account, DateTime lastUpdated, ILogger logger)
            {
                _logger = logger;
                var batchPage = GetContactPage(account, 1, lastUpdated);
                _totalResults = batchPage.TotalResults;
                _values = GenerateValues(account, lastUpdated, batchPage);
            }

            public int TotalResults { get { return _totalResults; } }

            public IObservable<IAccountContactSummary> Values { get { return _values; } }

            private IObservable<IAccountContactSummary> GenerateValues(IAccount account, DateTime lastUpdated, BatchOperationPage<IAccountContactSummary> batchPage)
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

            private IEnumerable<BatchOperationPage<IAccountContactSummary>> GetPages(IAccount account, DateTime lastUpdated, BatchOperationPage<IAccountContactSummary> batchPage)
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

            private BatchOperationPage<IAccountContactSummary> GetContactPage(IAccount account, int startIndex, DateTime lastUpdated)
            {
                _logger.Debug("GetContactPage({0}, {1}, {2:o})", account.AccountId, startIndex, lastUpdated);
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
                    _logger.Debug("Contacts - Received : {0}, NextPageStartIndex : {1}, TotalResults : {2}", 
                        contacts.Items.Count, contacts.NextPageStartIndex, contacts.TotalResults);
                    return contacts;
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "GetContactPage({0}, {1}, {2:o})", account.AccountId, startIndex, lastUpdated);
                    return BatchOperationPage<IAccountContactSummary>.Empty();
                }
            }
        }
    }
}
