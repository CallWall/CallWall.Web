using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;

namespace CallWall.Web.EventStore
{
    public sealed class EventStoreClient : IEventStoreClient, IDisposable
    {
        private static readonly UserCredentials AdminUserCredentials = new UserCredentials("admin", "changeit");
        private readonly ILogger _logger;
        private readonly Lazy<Task<IEventStoreConnection>> _conn;
        private bool _hasConnectionBegun;

        public EventStoreClient(IEventStoreConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            //_connectionFactory = connectionFactory;
            _conn = new Lazy<Task<IEventStoreConnection>>(() =>
                                                          {
                                                              _hasConnectionBegun = true;
                                                              return connectionFactory.Connect();
                                                          });

            _logger = loggerFactory.CreateLogger(GetType());
        }

        public IObservable<ResolvedEvent> GetNewEvents(string streamName)
        {
            return Observable.Create<ResolvedEvent>(async o =>
                {
                    var conn = await _conn.Value;

                    Action<EventStoreSubscription, ResolvedEvent> callback =
                        (arg1, arg2) => o.OnNext(arg2);

                    var subscription = await conn.SubscribeToStreamAsync(streamName, false, callback);

                    return subscription;
                });
        }

        public IObservable<ResolvedEvent> GetEvents(string streamName, int? fromVersion = null)
        {
            return Observable.Create<ResolvedEvent>(async o =>
            {
                var conn = await _conn.Value;

                Action<EventStoreCatchUpSubscription, ResolvedEvent> callback = (arg1, arg2) => o.OnNext(arg2);

                var subscription = conn.SubscribeToStreamFrom(streamName, fromVersion, true, callback);

                return Disposable.Create(() =>
                    {
                        try
                        {
                            subscription.Stop(TimeSpan.FromSeconds(2));
                        }
                        catch (Exception ex)
                        {
                            //HACK : Check this is fixed in newer vers of ES - LC
                            // https://groups.google.com/forum/#!topic/event-store/ZRQ9IRg1k5w
                            _logger.Warn(ex, "Failed to stop subscription.");
                        }
                    });
            });
        }

        public async Task<int> GetHeadVersion(string streamName)
        {
            var conn = await _conn.Value;

            var slice = await conn.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);
            if (slice.Status == SliceReadStatus.Success && slice.Events.Length == 1)
            {
                return slice.Events[0].OriginalEvent.EventNumber;
            }
            if (slice.Status == SliceReadStatus.StreamNotFound)
            {
                return ExpectedVersion.NoStream;//- 1;
            }
            throw new StreamDeletedException(streamName);
        }

        public async Task SaveEvent(string streamName, int expectedVersion, Guid eventId, string eventType, string jsonData, string jsonMetaData = null)
        {
            _logger.Trace("SaveEvent(" + streamName + ", " + expectedVersion + ", " + eventId + ", " + eventType + ")");
            var payload = Encoding.UTF8.GetBytes(jsonData);
            var metadata = jsonMetaData == null ? null : Encoding.UTF8.GetBytes(jsonMetaData);
            try
            {
                var conn = await _conn.Value;
                await conn.AppendToStreamAsync(streamName, expectedVersion, new EventData(eventId, eventType, true, payload, metadata));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to SaveEvent({0}, {1}, {2}, {3})", streamName, expectedVersion, eventId, eventType);
                throw;
            }
        }

        public async Task SaveBatch(string streamName, int expectedVersion, string eventType, string[] jsonData)
        {
            _logger.Trace("SaveBatch({0}, {1}, {2}, jsonData[{3}])", streamName, expectedVersion, eventType, jsonData.Length);
            var events = jsonData.Select(Encoding.UTF8.GetBytes)
                .Select(bin => new EventData(Guid.NewGuid(), eventType, true, bin, null));

            try
            {
                var conn = await _conn.Value;
                await conn.AppendToStreamAsync(streamName, expectedVersion, events);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to SaveBatch({0}, {1}, {2}, jsonData[{3}])", streamName, expectedVersion, eventType, jsonData.Length);
                throw;
            }
        }

        public async Task<IDisposable> AllEvents(Action<ResolvedEvent> onEventReceived)
        {
            var conn = await _conn.Value;
            try
            {
                //TODO: Handle the subscription dropped callback? -LC
                //  Potentially pass in a strategy for handling the subscription drop? -LC
                var subscription = await conn.SubscribeToAllAsync(true,
                    (eventStoreSubscription, resolvedEvent) => onEventReceived(resolvedEvent),
                    (eventStoreSubscription, dropReason, exception) =>
                    {
                        if (dropReason != SubscriptionDropReason.UserInitiated)
                            _logger.Error("Subscription was dropped '{0}' - Error: {1}", dropReason, exception);
                    },
                    AdminUserCredentials);
                _logger.Debug("Subscribed to all events");
                return subscription;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error subscribing to all events");
                conn.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (_hasConnectionBegun)
                _conn.Value.Dispose();
        }
    }
}