using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace CallWall.Web.EventStore
{
    public static class EventStoreExtensions
    {
        public static T Deserialize<T>(this RecordedEvent recordedEvent)
        {
            var data = recordedEvent.Data;
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}