using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace CallWall.Web.EventStore
{
    public abstract class DomainEventBase : NotificationBase, IDomainEventBase
    {
        #region Fields

        private readonly SingleAssignmentDisposable _eventSubscription = new SingleAssignmentDisposable();
        private readonly string _streamName;
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ILogger _logger;
        private int? _initialHeadVersion;
        private int _isRunning;
        private int _writeVersion = ExpectedVersion.NoStream;
        private DomainEventState _state = DomainEventState.Idle;


        #endregion

        protected DomainEventBase(IEventStoreClient eventStoreClient, ILoggerFactory loggerFactory, string streamName)
        {
            _streamName = streamName;
            _eventStoreClient = eventStoreClient;
            _logger = loggerFactory.CreateLogger(GetType());
            
            ReadVersion = ExpectedVersion.NoStream;
        }

        protected string StreamName { get { return _streamName; } }

        protected int ReadVersion { get; private set; }

        protected int WriteVersion { get { return _writeVersion; } }

        protected ILogger Logger { get { return _logger; } }


        protected IEventStoreClient EventStoreClient { get { return _eventStoreClient; } }

        public DomainEventState State
        {
            get { return _state; }
            private set
            {
                if (Equals(value, _state)) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        protected int InitialHeadVersion
        {
            get { return _initialHeadVersion.GetValueOrDefault(-3); }
        }

        public async Task Run()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
                return;
            _logger.Info("Running (Listening to '{0}')", _streamName);

            _initialHeadVersion = await EventStoreClient.GetHeadVersion(StreamName);
            _logger.Debug("{0} head is at version {1}", _streamName, InitialHeadVersion);

            var query = from evt in EventStoreClient.GetEvents(StreamName)
                        select evt;

            _eventSubscription.Disposable = query
                .Subscribe(
                    ReceiveEvent,
                    OnStreamError);

            SetState();
        }

        protected async Task WriteEvent(Guid eventId, string eventType, string eventData)
        {
            await EventStoreClient.SaveEvent(StreamName,
                _writeVersion,
                eventId,
                eventType,
                eventData);

            IncrementWriteVersion();
        }

        protected abstract void OnEventReceived(ResolvedEvent resolvedEvent);

        protected abstract void OnStreamError(Exception error);

        private void IncrementWriteVersion()
        {
            Interlocked.Increment(ref _writeVersion);
            using (QueueNotifications())
            {
                SetState();
                OnPropertyChanged("WriteVersion");
            }
        }

        private void ReceiveEvent(ResolvedEvent resolvedEvent)
        {
            _logger.Debug("{0}.Received({1}[{2}] {{ EventType = '{3}'}}",
                GetType().Name,
                resolvedEvent.OriginalEvent.EventStreamId, resolvedEvent.OriginalEvent.EventNumber,
                resolvedEvent.OriginalEvent.EventType);

            OnEventReceived(resolvedEvent);
            using (QueueNotifications())
            {
                ReadVersion = resolvedEvent.OriginalEventNumber;
                if (ReadVersion > WriteVersion)
                {
                    IncrementWriteVersion();
                }
                else
                {
                    SetState();
                }
            }
        }

        private void SetState()
        {
            _logger.Trace("Setting state. Current State : {0}", State);
            State = ReevaluateState();
            _logger.Trace("Setting state. New State : {0}", State);
        }
        private DomainEventState ReevaluateState()
        {
            _logger.Trace("Reevaluating State :InitialHeadVersion:{0} ReadVersion:{1} WriteVersion:{2}", InitialHeadVersion, ReadVersion, WriteVersion);
            if (ReadVersion < InitialHeadVersion)
            {
                return DomainEventState.LoadingFromHistory;
            }
            if (ReadVersion < WriteVersion)
            {
                return DomainEventState.BehindWrites;
            }
            return DomainEventState.Current;
        }

        public void Dispose()
        {
            _eventSubscription.Dispose();
            _logger.Info("{0} Disposed", GetType().Name);
        }
    }
}