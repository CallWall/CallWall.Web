using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly ILogger _logger;

        public GoogleAccountContactProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }
        public string Provider { get { return "Google"; } }

        public IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            if (account.Provider != Provider)
                return Observable.Empty<IAccountContactSummary>();

            return GetPages(account, lastUpdated)
                .SelectMany(batch => batch.Items)
                .Log(_logger, "GetContactsFeed(" + account.AccountId + ")");
        }

        public IObservable<IContactProfile> GetContactDetails(IEnumerable<ISession> session, string[] contactKeys)
        {
            //TODO: Implement Google GetContactDetails
            return Observable.Empty<IContactProfile>();
        }

        private IObservable<BatchOperationPage<IAccountContactSummary>> GetPages(IAccount account, DateTime lastUpdated)
        {
            return Observable.Create<BatchOperationPage<IAccountContactSummary>>(
                async (o, ct) =>
                {
                    var batchPage = await GetContactPage(account, 1, lastUpdated, ct);
                    o.OnNext(batchPage);
                    while (batchPage.NextPageStartIndex > 0)
                    {
                        //This was an issue, but its impact is reduced by increasing the maximum page size form the default of 25 to 1000. The upper limit maybe 9999, however the rest of CallWall would need to be designed to cater for those volumes too. -LC
                        await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
                        batchPage = await GetContactPage(account, batchPage.NextPageStartIndex, lastUpdated, ct);
                        o.OnNext(batchPage);
                    }
                    o.OnCompleted();
                });
        }

        private async Task<BatchOperationPage<IAccountContactSummary>> GetContactPage(IAccount account, int startIndex, DateTime lastUpdated, CancellationToken ct)
        {
            _logger.Debug("GetContactPage({0}, {1}, {2:o})", account.AccountId, startIndex, lastUpdated);
            var client = new HttpClient();

            var requestUriBuilder = new UriBuilder("https://www.google.com/m8/feeds/contacts/default/full");
            requestUriBuilder.AddQuery("access_token", HttpUtility.UrlEncode(account.CurrentSession.AccessToken))
                             .AddQuery("start-index", startIndex.ToString(CultureInfo.InvariantCulture))
                             .AddQuery("max-results", "1000");

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
                var response = await client.SendAsync(request, ct);
                //TODO: Handle Auth failure here. How can I refresh a token from here? -> Maybe throw and then have provider deal with it. It can then resubscribe once the session is refreshed. -LC
                response.EnsureSuccessStatusCode();
                var contentLength = response.Content.Headers.ContentLength;
                var contactResponse = await response.Content.ReadAsStringAsync();

                var translator = new GoogleContactProfileTranslator();
                var contacts = translator.Translate(contactResponse, account.CurrentSession.AccessToken, account);
                _logger.Debug("Contacts - Received : {0}, NextPageStartIndex : {1}, TotalResults : {2}, ContentLength (from Http header) : {3}",
                    contacts.Items.Count, contacts.NextPageStartIndex, contacts.TotalResults, contentLength);
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
