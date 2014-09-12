using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;
using Newtonsoft.Json;

namespace CallWall.Web.EventStore
{
    //No doubt this will have to change as we discover our requirements.
    public interface IEventStoreClient
    {
        //void SaveEvent(string streamName, string eventType, string jsonData, string jsonMetaData = null);
        //IObservable<string> GetNewEvents(string streamName);
        //IObservable<string> GetAllEvents(string streamName);
        //IObservable<ResolvedEvent> GetEvents(string streamName, int? fromVersion);
        //Task<string> GetHead(string streamName);

        IObservable<ResolvedEvent> AllEvents();
        IObservable<ResolvedEvent> GetEvents(string streamName, int? fromVersion = null);
        IObservable<string> GetNewEvents(string streamName);
        
        Task<int> GetHeadVersion(string streamName);
        Task SaveEvent(string streamName, int expectedVersion, Guid eventId, string eventType, string jsonData,
            string jsonMetaData = null);      
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
                             .Select(JsonConvert.DeserializeObject<T>);
        }
        public static IObservable<T> GetEvents<T>(this IEventStoreClient eventStoreClient, string streamName)
        {
            return eventStoreClient.GetEvents(streamName)
                .Select(re=>Encoding.UTF8.GetString(re.OriginalEvent.Data))
                .Select(JsonConvert.DeserializeObject<T>);
        }

        //public static IObservable<T> GetHead<T>(this IEventStoreClient eventStoreClient, string streamName)
        //{
        //    return eventStoreClient.GetHead(streamName)
        //                     .ToObservable()
        //                     .Select(JsonConvert.DeserializeObject<T>);
        //}
    }
}