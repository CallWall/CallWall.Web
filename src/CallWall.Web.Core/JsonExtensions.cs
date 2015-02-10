using Newtonsoft.Json;

namespace CallWall.Web
{
    //Because I don't want to have to remember how to use JSON.NET. -LC
    public static class JsonExtensions
    {
        public static string ToJson(this object source)
        {
            return JsonConvert.SerializeObject(source);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}