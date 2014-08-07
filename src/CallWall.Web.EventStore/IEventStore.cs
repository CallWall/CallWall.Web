using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;
using Newtonsoft.Json;

namespace CallWall.Web.EventStore
{
    //No doubt this will have to change as we discover our requirements.
    internal interface IEventStore
    {
        void SaveEvent(string streamName, string eventType, string jsonData, string jsonMetaData = null);
        IObservable<string> GetNewEvents(string streamName);
        IObservable<string> GetAllEvents(string streamName);
        IObservable<ResolvedEvent> GetEvents(string streamName, int? fromVersion);
        Task<string> GetHead(string streamName);
    }

    internal static class EventStoreEx
    {
        public static void SaveEvent<T>(this IEventStore eventStore, string streamName, T data)
        {
            eventStore.SaveEvent(streamName, data.GetType().Name, data.ToJson());
        }

        public static IObservable<T> GetNewEvents<T>(this IEventStore eventStore, string streamName)
        {
            return eventStore.GetNewEvents(streamName)
                             .Select(JsonConvert.DeserializeObject<T>);
        }
        public static IObservable<T> GetAllEvents<T>(this IEventStore eventStore, string streamName)
        {
            return eventStore.GetAllEvents(streamName)
                             .Select(JsonConvert.DeserializeObject<T>);
        }

        public static IObservable<T> GetHead<T>(this IEventStore eventStore, string streamName)
        {
            return eventStore.GetHead(streamName)
                             .ToObservable()
                             .Select(JsonConvert.DeserializeObject<T>);
        }
    }
}