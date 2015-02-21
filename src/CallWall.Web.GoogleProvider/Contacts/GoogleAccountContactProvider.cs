using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CallWall.Web.Domain;
using CallWall.Web.Http;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider.Contacts
{
    internal sealed class GoogleAccountContactProvider : IAccountContactProvider
    {
        private readonly IGoogleContactProfileTranslator _translator;
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public GoogleAccountContactProvider(IGoogleContactProfileTranslator translator, IHttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _translator = translator;
            _httpClient = httpClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public string Provider { get { return Constants.ProviderName; } }

        public IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            if (account.Provider != Provider)
                return Observable.Empty<IAccountContactSummary>();

            return (from groups in GetGroups(account)
                    from page in GetContactPages(account, lastUpdated, groups)
                    from item in page.Items
                    select item)
                .Log(_logger, "GetContactsFeed(" + account.AccountId + ")");
        }

        private IObservable<Dictionary<string, string>> GetGroups(IAccount account)
        {
            //TODO: This should fetch any extra pages of groups
            //TODO: The groups can be cached as they are related to the logged in user. I would imagine that we can safely cache for 1minute.
            return (
                       from request in CreateContactGroupRequestParams(account.CurrentSession.AccessToken).AsObservable()
                       from response in _httpClient.GetResponse(request)
                       select _translator.ToGroupDictionary(response)
                   )
                .Log(_logger, "Account Contact Groups")
                .Take(1);
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
            var request = CreateContactPageRequest(account, startIndex, lastUpdated);
            return await _httpClient.GetResponse(request)
                .Select(contactResponse =>_translator.Translate(contactResponse, account.CurrentSession.AccessToken, account, groups))
                .Do(
                    contacts =>
                        _logger.Debug("Contacts - Received : {0}, NextPageStartIndex : {1}, TotalResults : {2}",
                            contacts.Items.Count, contacts.NextPageStartIndex, contacts.TotalResults),
                    ex =>
                        _logger.Error(ex, "GetContactPage({0}, {1}, {2:o})", account.AccountId, startIndex,lastUpdated))
                .Catch((Exception ex) => Observable.Return(BatchOperationPage<IAccountContactSummary>.Empty()))
                .Take(1)
                .ToTask(ct);
        }

        private static HttpRequestParameters CreateContactGroupRequestParams(string accessToken)
        {
            var param = new HttpRequestParameters(@"https://www.google.com/m8/feeds/groups/default/full");
            //TODO:Validate that I should be passing this in the query string. Surly I want this encoded in the POST stream -LC
            param.QueryStringParameters.Add("access_token", accessToken);
            param.Headers.Add("GData-Version", "3.0");

            return param;
        }

        private static HttpRequestParameters CreateContactPageRequest(IAccount account, int startIndex, DateTime lastUpdated)
        {
            //See https://developers.google.com/google-apps/contacts/v3/reference?hl=es#Parameters for reference on query API.
            var request = new HttpRequestParameters("https://www.google.com/m8/feeds/contacts/default/full");
            request.QueryStringParameters.Add("access_token", HttpUtility.UrlEncode(account.CurrentSession.AccessToken));
            request.QueryStringParameters.Add("start-index", startIndex.ToString(CultureInfo.InvariantCulture));
            request.QueryStringParameters.Add("max-results", "1000");
            request.QueryStringParameters.Add("showdeleted", "true");

            if (lastUpdated != default(DateTime))
            {
                var formattedDate = lastUpdated.ToString("yyyy-MM-ddT00:00:00");
                request.QueryStringParameters.Add("updated-min", formattedDate);
            }
            request.Headers.Add("GData-Version", "3.0");
            return request;
        }
    }
}
