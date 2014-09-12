using System;
using System.Diagnostics;
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

        private readonly string _streamName;
        
        private readonly SingleAssignmentDisposable _eventSubscription = new SingleAssignmentDisposable();
        private readonly Lazy<Task<int>> _initialHeadVersion; 
        private int _isRunning;
        private int _writeVersion = ExpectedVersion.NoStream;
        private DomainEventState _state = DomainEventState.Idle;
        private readonly IEventStoreClient _eventStoreClient;

        #endregion

        protected DomainEventBase(IEventStoreClient eventStoreClient, string streamName)
        {
            _streamName = streamName;
            _eventStoreClient = eventStoreClient;
            _initialHeadVersion =  new Lazy<Task<int>>(()=>EventStoreClient.GetHeadVersion(StreamName));
            ReadVersion = ExpectedVersion.NoStream;
        }
        
        protected string StreamName { get { return _streamName; } }

        protected int ReadVersion { get; private set; }

        protected int WriteVersion { get { return _writeVersion; } }


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

        public void Run()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
                return;
            Trace.WriteLine("Running " + GetType().Name);

            var query = from _ in _initialHeadVersion.Value.ToObservable()
                        from evt in EventStoreClient.GetEvents(StreamName)
                        select evt;

            _eventSubscription.Disposable = query
                .Subscribe(
                    ReceiveEvent,
                    OnStreamError);
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
                State = ReevaluateState();
                OnPropertyChanged("WriteVersion");    
            }
        }

        private void ReceiveEvent(ResolvedEvent resolvedEvent)
        {
            var logMsg = string.Format("{0}.Received({1}[{2}] {{ EventType = '{3}'}}",
                                GetType().Name,
                                resolvedEvent.OriginalEvent.EventStreamId, resolvedEvent.OriginalEvent.EventNumber,
                                resolvedEvent.OriginalEvent.EventType);
            Trace.WriteLine(logMsg);
            //Trace.WriteLine("OnEventReceived " + resolvedEvent.OriginalEvent.EventStreamId + " " + resolvedEvent.OriginalEvent.EventType);
            
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
                    State = ReevaluateState();        
                }
            }
        }

        private DomainEventState ReevaluateState()
        {
            if (ReadVersion < _initialHeadVersion.Value.Result)
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
        }
    }
}