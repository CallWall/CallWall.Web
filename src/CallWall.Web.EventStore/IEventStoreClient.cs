using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace CallWall.Web.EventStore
{
    //No doubt this will have to change as we discover our requirements.
    public interface IEventStoreClient
    {
        Task<IDisposable> AllEvents(Action<ResolvedEvent> onEventReceived);
        IObservable<ResolvedEvent> GetEvents(string streamName, int? fromVersion = null);
        IObservable<ResolvedEvent> GetNewEvents(string streamName);
        
        Task<int> GetHeadVersion(string streamName);
        Task SaveEvent(string streamName, int expectedVersion, Guid eventId, string eventType, string jsonData,
            string jsonMetaData = null);

        Task SaveBatch(string streamName, int expectedVersion, string eventType, string[] jsonData);
    }

    internal static class EventStoreEx
    {
        public static void SaveEvent<T>(this IEventStoreClient eventStoreClient, string streamName, int expectedVersion, Guid eventId, T data)
        {
            eventStoreClient.SaveEvent(streamName, expectedVersion, eventId, data.GetType().Name, data.ToJson());
        }

        public static IObservable<T> GetNewEvents<T>(this IEventStoreClient eventStoreClient, string streamName)
        {
            return eventStoreClient.GetNewEvents(streamName)
                .Select(re=>re.OriginalEvent.Data)
                .Select(Encoding.UTF8.GetString)
                .Select(JsonConvert.DeserializeObject<T>);
        }
        public static IObservable<T> GetEvents<T>(this IEventStoreClient eventStoreClient, string streamName)
        {
            return eventStoreClient.GetEvents(streamName)
                .Select(re=>Encoding.UTF8.GetString(re.OriginalEvent.Data))
                .Select(JsonConvert.DeserializeObject<T>);
        }
    }
}