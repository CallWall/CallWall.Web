using System;
using System.Data;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;

namespace CallWall.Web.EventStore
{
    public sealed class EventStoreClient : IEventStoreClient
    {
        private readonly IEventStoreConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        public EventStoreClient(IEventStoreConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            _connectionFactory = connectionFactory;
            _logger = loggerFactory.CreateLogger(GetType());
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
            _logger.Trace("SaveEvent(" + streamName + ", " + expectedVersion + ", " + eventId  + ", " + eventType + ")");
            var payload = Encoding.UTF8.GetBytes(jsonData);
            var metadata = jsonMetaData == null ? null : Encoding.UTF8.GetBytes(jsonMetaData);
            using (var conn = _connectionFactory.Connect())
            {
                await conn.AppendToStreamAsync(streamName, expectedVersion, new EventData(eventId, eventType, true, payload, metadata));
            }
        }


        private static readonly UserCredentials UserCredentials = new UserCredentials("admin", "changeit");

        public IObservable<ResolvedEvent> AllEvents()
        {
            return Observable.Create<ResolvedEvent>(async o =>
            {
                Action<EventStoreSubscription, ResolvedEvent> callback =
                    (eventStoreSubscription, resolvedEvent) =>
                    {
                        //var logMsg = string.Format("{0}.Received({1}[{2}] {{ EventType = '{3}'}}",
                        //    typeName,
                        //    resolvedEvent.OriginalEvent.EventStreamId,
                        //    resolvedEvent.OriginalEvent.EventNumber,
                        //    resolvedEvent.OriginalEvent.EventType);
                        //Trace.WriteLine(logMsg);
                        o.OnNext(resolvedEvent);
                    };

                var conn = _connectionFactory.Connect();

                try
                {
                    //TODO: Handle the subscription dropped callback? -LC
                    var subscription = await conn.SubscribeToAllAsync(true, callback, null, UserCredentials);
                    //Trace.WriteLine(typeName + " is subscribed to all events");
                    return new CompositeDisposable(subscription, conn);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error subscribing to all events.");
                    Trace.TraceError(e.ToString());
                    conn.Dispose();
                    return Disposable.Empty;
                }
            });

        }
    }
}