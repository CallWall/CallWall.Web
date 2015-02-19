using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Providers.Contacts;
using CallWall.Web.Http;
using CallWall.Web.Providers;
using HttpClient = System.Net.Http.HttpClient;

namespace CallWall.Web.GoogleProvider.Contacts
{
    internal sealed class GoogleAccountContactProvider : IAccountContactProvider
    {
        private readonly IHttpClient _httpClient;
        private static readonly GoogleContactProfileTranslator Translator = new GoogleContactProfileTranslator();
        private readonly ILogger _logger;

        public GoogleAccountContactProvider(IHttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public string Provider { get { return Constants.ProviderName; } }

        public IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            if (account.Provider != Provider)
                return Observable.Empty<IAccountContactSummary>();

            //TODO: Need to make a call here to get all the Groups for the Account. 
            //  This feed needs to be combined with the contact feed, and then the contacts enriched with the correct tags. (as per old code)


            return (from groups in GetGroups(account)
                from page in GetContactPages(account, lastUpdated, groups)
                from item in page.Items
                select item)
                .Log(_logger, "GetContactsFeed(" + account.AccountId + ")");
        }


        private IObservable<BatchOperationPage<IAccountContactSummary>> GetContactPages(IAccount account, DateTime lastUpdated, Dictionary<string, string> groups)
        {
            return Observable.Create<BatchOperationPage<IAccountContactSummary>>(
                async (o, ct) =>
                {
                    var batchPage = await GetContactPage(account, 1, lastUpdated, groups, ct);
                    o.OnNext(batchPage);
                    while (batchPage.NextPageStartIndex > 0)
                    {
                        //This was an issue, but its impact is reduced by increasing the maximum page size form the default of 25 to 1000. The upper limit maybe 9999, however the rest of CallWall would need to be designed to cater for those volumes too. -LC
                        await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
                        batchPage = await GetContactPage(account, batchPage.NextPageStartIndex, lastUpdated, groups, ct);
                        o.OnNext(batchPage);
                    }
                    o.OnCompleted();
                });
        }

        private async Task<BatchOperationPage<IAccountContactSummary>> GetContactPage(IAccount account, int startIndex, DateTime lastUpdated, Dictionary<string, string> groups, CancellationToken ct)
        {
            _logger.Debug("GetContactPage({0}, {1}, {2:o})", account.AccountId, startIndex, lastUpdated);
            //TODO: Use the IHttpClient (will allow for logging etc). -LC
            var client = new HttpClient();

            var request = CreateContactPageRequest(account, startIndex, lastUpdated);

            //TODO: Add error handling (not just exceptions but also non 200 responses -LC
            try
            {
                var response = await client.SendAsync(request, ct);
                //TODO: Handle Auth failure here. How can I refresh a token from here? -> Maybe throw and then have provider deal with it. 
                //  It can then re-subscribe once the session is refreshed. -LC
                response.EnsureSuccessStatusCode();
                var contentLength = response.Content.Headers.ContentLength;
                var contactResponse = await response.Content.ReadAsStringAsync();

                var contacts = Translator.Translate(contactResponse, account.CurrentSession.AccessToken, account, groups);
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

        private static HttpRequestMessage CreateContactPageRequest(IAccount account, int startIndex, DateTime lastUpdated)
        {
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
            return request;
        }

        private IObservable<Dictionary<string,string>> GetGroups(IAccount account)
        {
            //TODO: This should fetch any extra pages of groups
            //TODO: The groups can be cached as they are related to the logged in user. I would imagine that we can safely cache for 1minute.
            return (
                       from request in CreateContactGroupRequestParams(account.CurrentSession.AccessToken).AsObservable()
                       from response in _httpClient.GetResponse(request)
                       select Translator.ToGroupDictionary(response)
                   )
                .Log(_logger, "Account Contact Groups")
                .Take(1);
        }

        private static HttpRequestParameters CreateContactGroupRequestParams(string accessToken)
        {
            var param = new HttpRequestParameters(@"https://www.google.com/m8/feeds/groups/default/full");
            //TODO:Validate that I should be passing this in the query string. Surly I want this encoded in the POST stream -LC
            param.QueryStringParameters.Add("access_token", accessToken);
            param.Headers.Add("GData-Version", "3.0");

            return param;
        }
    }
}
