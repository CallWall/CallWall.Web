using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CallWall.Web.Providers.Google
{
    public static class SessionExtensions
    {
        public static string ToJson(this ISession session)
        {
            var jObject = JObject.FromObject(session);
            var json = jObject.ToString(Formatting.None);
            return json;
        }

        public static ISession FromJson(this string session)
        {
            var json = JObject.Parse(session);

            var authorizedResources = json["AuthorizedResources"].ToObject<IEnumerable<Uri>>();

            return new Session(
                (string)json["AccessToken"],
                (string)json["RefreshToken"],
                (DateTimeOffset)json["Expires"],
                authorizedResources);
        }
    }
}