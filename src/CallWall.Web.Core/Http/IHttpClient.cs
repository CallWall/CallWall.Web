using System;

namespace CallWall.Web.Http
{
    public interface IHttpClient
    {
        IObservable<string> GetResponse(HttpRequestParameters request);
    }
}