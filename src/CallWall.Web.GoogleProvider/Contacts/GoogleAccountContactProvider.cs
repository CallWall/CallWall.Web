using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CallWall.Web.Domain;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider.Contacts
{
    internal sealed class GoogleAccountContactProvider : IAccountContactProvider
    {
        private static readonly GoogleContactProfileTranslator Translator = new GoogleContactProfileTranslator();
        private readonly ILogger _logger;

        public GoogleAccountContactProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public string Provider { get { return Constants.ProviderName; } }

        public IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            if (account.Provider != Provider)
                return Observable.Empty<IAccountContactSummary>();

            return GetPages(account, lastUpdated)
                .SelectMany(batch => batch.Items)
                .Log(_logger, "GetContactsFeed(" + account.AccountId + ")");
        }








        public IObservable<IContactProfile> GetContactDetails(User user, string[] contactKeys)
        {
            //For each relevant account, for each contact, make a query to get contact details.
            var query = from googleAccount in user.Accounts.Where(acc => acc.Provider == Provider)
                        from contactKey in contactKeys
                        select GetContactDetails(googleAccount, contactKey);
            
            //Flatten the results, and send back to be aggregated by layer above.
            return query.Merge();
        }

        private IObservable<IContactProfile> GetContactDetails(IAccount account, string contactKey)
        {
            //TODO: Implement Google GetContactDetails
            //Should just be a case of making the same call as below but with a filter, not an open query -LC

            //Delegate this to the EventStore/Repository. -LC
            //This could issue a Refresh request (specific to the providerContactId) which would update the EventStore -LC

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

            //See https://developers.google.com/google-apps/contacts/v3/reference?hl=es#Parameters for reference on query API.
            var requestUriBuilder = new UriBuilder("https://www.google.com/m8/feeds/contacts/default/full");
            requestUriBuilder.AddQuery("access_token", HttpUtility.UrlEncode(account.CurrentSession.AccessToken))
                             .AddQuery("start-index", startIndex.ToString(CultureInfo.InvariantCulture))
                             .AddQuery("max-results", "1000")
                             .AddQuery("showdeleted", "true");  

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

                var contacts = Translator.Translate(contactResponse, account.CurrentSession.AccessToken, account);
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
