using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using System.Threading;

namespace CallWall.Web.EventStore
{
    public abstract class AllEventListenerBase : IDisposable, IRunnable
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ILogger _logger;
        private readonly EventLoopScheduler _eventLoopScheduler;
        private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();
        private int _isRunning;

        protected AllEventListenerBase(IEventStoreClient eventStoreClient, ILoggerFactory loggerFactory)
        {
            _eventStoreClient = eventStoreClient;
            _logger = loggerFactory.CreateLogger(GetType());
            _eventLoopScheduler = new EventLoopScheduler(ts => new Thread(ts) { Name = string.Format("{0}.AllEventListener", GetType().Name) });
        }

        protected ILogger Logger
        {
            get { return _logger; }
        }

        //TODO: This needs to have some error handling. What should happen if we cant connect? -LC
        public async Task Run()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
                return;

            try
            {
                _subscription.Disposable = await _eventStoreClient.AllEvents(OnEventReceivedImpl);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error subscribing to all events");
                throw;
            }


            _logger.Info("Running (Listening to all events)");
        }

        private void OnEventReceivedImpl(ResolvedEvent resolvedEvent)
        {
            _logger.Trace("{0} Received event and scheduling onto ELS - {1}[{2}] {{ EventType = '{3}'}}",
                                GetType().Name,
                                resolvedEvent.OriginalEvent.EventStreamId, resolvedEvent.OriginalEvent.EventNumber,
                                resolvedEvent.OriginalEvent.EventType);
            _eventLoopScheduler.Schedule(() =>
            {
                _logger.Trace("{0}.OnEventReceived({1}[{2}] {{ EventType = '{3}'}}",
                                GetType().Name,
                                resolvedEvent.OriginalEvent.EventStreamId, resolvedEvent.OriginalEvent.EventNumber,
                                resolvedEvent.OriginalEvent.EventType);
                OnEventReceived(resolvedEvent);
            });
        }
        protected abstract void OnEventReceived(ResolvedEvent resolvedEvent);

        protected async Task SaveEvent(string streamName, int expectedVersion, Guid eventId, string eventType, string jsonData, string jsonMetaData = null)
        {
            await _eventStoreClient.SaveEvent(streamName, expectedVersion, eventId, eventType, jsonData);
        }

        protected async Task SaveBatch(string streamName, int expectedVersion, string eventType, string[] jsonData)
        {
            await _eventStoreClient.SaveBatch(streamName, expectedVersion, eventType, jsonData);
        }

        public void Dispose()
        {
            OnDispose(true);
        }

        protected virtual void OnDispose(bool isDisposing)
        {
            _subscription.Dispose();
            _eventLoopScheduler.Dispose();
            _logger.Info("{0} Disposed", GetType().Name);
        }
    }

    //public abstract class ProjectionListenerBase : IDisposable
    //{
    //    private readonly IEventStoreConnectionFactory _connectionFactory;
    //    private int _isRunning;
    //    private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();

    //    protected ProjectionListenerBase(IEventStoreConnectionFactory connectionFactory)
    //    {
    //        _connectionFactory = connectionFactory;
    //    }

    //    public void Run()
    //    {
    //        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
    //            return;

    //        var query = Observable.Create<ResolvedEvent>(
    //            async o =>
    //            {
    //                var projectionsManager = new ProjectionsManager(new DebugLogger(), new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113));
    //                const string projStr = "fromCategory('12-testentity').foreachStream().whenAny(function(s, e) { linkTo('test-12', e) })";
    //                var credentials = new UserCredentials("admin", "changeit");
    //                projectionsManager.CreateContinuous("$test-12", projStr, credentials);

    //                projectionsManager.Enable("$test-12", credentials);

    //            });

    //        _subscription.Disposable = query.Subscribe(OnEventReceived);
    //    }

    //    protected abstract void OnEventReceived(ResolvedEvent resolvedEvent);

    //    public void Dispose()
    //    {
    //        OnDispose(true);
    //    }

    //    protected virtual void OnDispose(bool isDisposing)
    //    {
    //        _subscription.Dispose();
    //    }
    //}
}