using System;
using System.Data;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

namespace CallWall.Web.EventStore
{
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
                var conn = _connectionFactory.Connect();

                Action<EventStoreSubscription, ResolvedEvent> callback = (arg1, arg2) => o.OnNext(arg2.OriginalEvent.Data);

                var subscription = conn.SubscribeToStream(streamName, false, callback);

                return new CompositeDisposable(subscription, conn);
            })
            .Select(Encoding.UTF8.GetString);
        }

        [Obsolete("Use GetEvents and don't provide a version/EventId")]
        public IObservable<string> GetAllEvents(string streamName)
        {
            return Observable.Create<byte[]>(o =>
            {
                var conn = _connectionFactory.Connect();

                Action<EventStoreCatchUpSubscription, ResolvedEvent> callback = (arg1, arg2) => o.OnNext(arg2.OriginalEvent.Data);
                
                var subscription = conn.SubscribeToStreamFrom(streamName, StreamPosition.Start, false, callback);

                return new CompositeDisposable(Disposable.Create(() => subscription.Stop(TimeSpan.FromSeconds(2))), conn);
            })
            .Select(Encoding.UTF8.GetString);
        }

        public IObservable<ResolvedEvent> GetEvents(string streamName, int? fromVersion = null)
        {
            return Observable.Create<ResolvedEvent>(o =>
            {
                var conn = _connectionFactory.Connect();

                Action<EventStoreCatchUpSubscription, ResolvedEvent> callback = (arg1, arg2) => o.OnNext(arg2);

                var subscription = conn.SubscribeToStreamFrom(streamName, fromVersion, true, callback);

                return new CompositeDisposable(Disposable.Create(() => subscription.Stop(TimeSpan.FromSeconds(2))), conn);
            });
        }

        [Obsolete("Use GetHeadVersion now", true)]
        public async Task<string> GetHead(string streamName)
        {
            using (var conn = _connectionFactory.Connect())
            {
                var slice = await conn.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);
                if (slice.Status == SliceReadStatus.Success && slice.Events.Length == 1)
                {
                    var binData = slice.Events[0].OriginalEvent.Data;
                    var json = Encoding.UTF8.GetString(binData);
                    return json;
                }
                var error = string.Format("Failed to read from the head of the stream '{0}' : {1}", slice.Stream, slice.Status);
                throw new MissingPrimaryKeyException(error);
            }
        }

        public async Task<int> GetHeadVersion(string streamName)
        {
            using (var conn = _connectionFactory.Connect())
            {
                var slice = await conn.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);
                if (slice.Status == SliceReadStatus.Success && slice.Events.Length == 1)
                {
                    return slice.Events[0].OriginalEvent.EventNumber;
                }
                if (slice.Status == SliceReadStatus.StreamNotFound)
                {
                    return -1;
                }
                throw new StreamDeletedException(streamName);
            }
        }

        [Obsolete("Use the new overload now", true)]
        public void SaveEvent(string streamName, string eventType, string jsonData, string jsonMetaData = null)
        {
            var payload = Encoding.UTF8.GetBytes(jsonData);
            var metadata = jsonMetaData == null ? null : Encoding.UTF8.GetBytes(jsonMetaData);
            using (var conn = _connectionFactory.Connect())
            {
                conn.AppendToStream(streamName, ExpectedVersion.Any, new EventData(Guid.NewGuid(), eventType, true, payload, metadata));
            }
        }
        public async Task SaveEvent(string streamName, int expectedVersion, Guid eventId, string eventType, string jsonData, string jsonMetaData = null)
        {
            Trace.WriteLine("SaveEvent(" + streamName + ", " + expectedVersion + ", " + eventId  + ", " + eventType + ")");
            var payload = Encoding.UTF8.GetBytes(jsonData);
            var metadata = jsonMetaData == null ? null : Encoding.UTF8.GetBytes(jsonMetaData);
            using (var conn = _connectionFactory.Connect())
            {
                await conn.AppendToStreamAsync(streamName, expectedVersion, new EventData(eventId, eventType, true, payload, metadata));
            }
        }
    }
}