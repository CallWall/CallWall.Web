using System;
using System.Reactive.Linq;
using CallWall.Web.Contracts;
using CallWall.Web.Http;

namespace CallWall.Web.GoogleProvider.Providers.Contacts
{
    public static class HttpParams
    {
        public static HttpRequestParameters CreateRequestParams(IPersonalIdentifier personalIdentifier, string accessToken)
        {
            var param = CreateRequestParams(accessToken);

            var query = personalIdentifier.Value ?? string.Empty;
            param.QueryStringParameters.Add("q", query);

            return param;
        }

        public static HttpRequestParameters CreateRequestParams(string accessToken)
        {
            var param = new HttpRequestParameters(@"https://www.google.com/m8/feeds/contacts/default/full");
            //TODO:Validate that I should be passing this in the query string. Surly I want this encoded in the POST stream -LC
            param.QueryStringParameters.Add("access_token", accessToken);
            param.Headers.Add("GData-Version", "3.0");
            return param;
        }

        public static HttpRequestParameters CreateContactGroupRequestParams(string accessToken)
        {
            var param = new HttpRequestParameters(@"https://www.google.com/m8/feeds/groups/default/full");
            //TODO:Validate that I should be passing this in the query string. Surly I want this encoded in the POST stream -LC
            param.QueryStringParameters.Add("access_token", accessToken);
            param.Headers.Add("GData-Version", "3.0");

            return param;
        }

        public static IObservable<HttpRequestParameters> AsObservable(this HttpRequestParameters httpRequestParameters)
        {
            return Observable.Return(httpRequestParameters);
        }
    }
}