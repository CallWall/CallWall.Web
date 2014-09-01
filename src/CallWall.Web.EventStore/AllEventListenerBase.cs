using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace CallWall.Web.EventStore
{
    public abstract class AllEventListenerBase : IDisposable
    {
        private readonly IEventStoreConnectionFactory _connectionFactory;
        private int _isRunning;
        private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();

        protected AllEventListenerBase(IEventStoreConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Run()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
                return;
            var typeName = GetType().Name;
            Trace.WriteLine("Running " + typeName);

            var query = Observable.Create<ResolvedEvent>(async o =>
                {
                    Action<EventStoreSubscription, ResolvedEvent> callback =
                        (eventStoreSubscription, resolvedEvent) =>
                        {
                            var logMsg = string.Format("{0}.Received({1}[{2}] {{ EventType = '{3}'}}",
                                typeName,
                                resolvedEvent.OriginalEvent.EventStreamId, resolvedEvent.OriginalEvent.EventNumber,
                                resolvedEvent.OriginalEvent.EventType);
                            Trace.WriteLine(logMsg);
                            o.OnNext(resolvedEvent);
                        };

                    var conn = _connectionFactory.Connect();

                    //TODO: Handle the subscription dropped callback? -LC
                    var subscription = await conn.SubscribeToAllAsync(true, callback,null, new UserCredentials("admin", "changeit"));

                    return new CompositeDisposable(subscription, conn);
                });

            _subscription.Disposable = query.Subscribe(OnEventReceived);
        }

        protected abstract void OnEventReceived(ResolvedEvent resolvedEvent);

        public void Dispose()
        {
            OnDispose(true);
        }

        protected virtual void OnDispose(bool isDisposing)
        {
            _subscription.Dispose();
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