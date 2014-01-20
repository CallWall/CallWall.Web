using System;

namespace CallWall.Web.Providers
{
    public static class UriExtensions
    {
        public static string AddQuery(this string uri, string param, string value)
        {
            return new UriBuilder(uri).AddQuery(param, value).Uri.ToString();
        }
        public static UriBuilder AddQuery(this UriBuilder baseUri, string param, string value)
        {
            var queryToAppend = string.Format("{0}={1}", param, value);
            if (baseUri.Query.Length > 1)
                baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
            else
                baseUri.Query = queryToAppend;
            return baseUri;
        }
    }
}