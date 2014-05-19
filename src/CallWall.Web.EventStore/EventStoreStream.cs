using System;
using System.Data;
using System.Reactive.Disposables;
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
    public interface IEventStore
    {
        void SaveEvent(string streamName, string eventType, string jsonData, string jsonMetaData = null);
        IObservable<string> GetNewEvents(string streamName);
        IObservable<string> GetAllEvents(string streamName);
        Task<string> GetHead(string streamName);
    }

    public static class EventStoreEx
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

    public sealed class EventStore : IEventStore
    {
        private readonly IEventStoreConnectionFactory _connectionFactory;

        public EventStore(IEventStoreConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        
        public IObservable<string> GetNewEvents(string streamName)
        {
            return Observable.Create<byte[]>(o =>
                {
                    var conn = _connectionFactory.CreateConnection();

                    Action<EventStoreSubscription, ResolvedEvent> callback = (arg1, arg2) => o.OnNext(arg2.OriginalEvent.Data);

                    var subscription = conn.SubscribeToStream(streamName, false, callback);

                    return new CompositeDisposable(subscription, conn);
                })
               .Select(Encoding.UTF8.GetString);
        }

        public IObservable<string> GetAllEvents(string streamName)
        {
            return Observable.Create<byte[]>(o =>
                {               
                    var conn = _connectionFactory.CreateConnection();

                    Action<EventStoreCatchUpSubscription, ResolvedEvent> callback = (arg1, arg2) => o.OnNext(arg2.OriginalEvent.Data);

                    var subscription = conn.SubscribeToStreamFrom(streamName, StreamPosition.Start, false, callback);
                
                    return new CompositeDisposable(Disposable.Create(()=>subscription.Stop(TimeSpan.FromSeconds(2))), conn);
                })
                .Select(Encoding.UTF8.GetString);
        }

        public async Task<string> GetHead(string streamName)
        {
            using (var conn = _connectionFactory.CreateConnection())
            {
                var slice = await conn.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);
                if (slice.Status == SliceReadStatus.Success && slice.Events.Length == 1)
                {
                    var binData = slice.Events[0].OriginalEvent.Data;
                    var json = Encoding.UTF8.GetString(binData);
                    return json;
                }
            }
            var error = string.Format("Failed to read from the head of the stream '{0}' : {1}", slice.Stream, slice.Status);
            throw new MissingPrimaryKeyException(error);
        }

        public void SaveEvent(string streamName, string eventType, string jsonData, string jsonMetaData = null)
        {
            var payload = Encoding.UTF8.GetBytes(jsonData);
            var metadata = jsonMetaData==null ? null : Encoding.UTF8.GetBytes(jsonMetaData);
            using (var conn = _connectionFactory.CreateConnection())
            {
                conn.AppendToStream(streamName, ExpectedVersion.Any, new EventData(Guid.NewGuid(), eventType, true, payload, metadata));    
            }            
        }
    }
}