using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CallWall.Web.EventStore.Tests
{
    public interface IDomainEventBase : INotifyPropertyChanged, IRunnable, IDisposable
    {
        DomainEventState State { get; }
    }
    public abstract class DomainEventBase : NotificationBase, IDomainEventBase
    {
        #region Fields

        private readonly string _streamName;
        protected readonly EventStore _eventStore;
        private readonly SingleAssignmentDisposable _eventSubscription = new SingleAssignmentDisposable();
        private readonly Lazy<Task<int>> _initialHeadVersion; 
        private int _isRunning;
        private int _writeVersion = ExpectedVersion.NoStream;
        private DomainEventState _state = DomainEventState.Idle;

        #endregion

        protected DomainEventBase(IEventStoreConnectionFactory connectionFactory, string streamName)
        {
            _streamName = streamName;
            _eventStore = new EventStore(connectionFactory);
            _initialHeadVersion =  new Lazy<Task<int>>(()=>_eventStore.GetHeadVersion(StreamName));
            ReadVersion = ExpectedVersion.NoStream;
        }

        protected string StreamName { get { return _streamName; } }

        protected int ReadVersion { get; private set; }

        protected int WriteVersion { get { return _writeVersion; } }

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
            
            var query = from _ in _initialHeadVersion.Value.ToObservable()
                        from evt in _eventStore.GetEvents(StreamName)
                        select evt;

            _eventSubscription.Disposable = query
                .Subscribe(
                    ReceiveEvent,
                    OnStreamError);
        }

        protected T Deserialize<T>(RecordedEvent recordedEvent)
        {
            var data = recordedEvent.Data;
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }

        protected async Task WriteEvent(Guid eventId, string eventType, string eventData)
        {
            await _eventStore.SaveEvent(StreamName,
                _writeVersion,
                eventId,
                eventType,
                eventData);

            IncrementWriteVersion();
        }

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

        protected abstract void OnEventReceived(ResolvedEvent resolvedEvent);

        protected abstract void OnStreamError(Exception error);


        public void Dispose()
        {
            _eventSubscription.Dispose();
        }
    }

    public abstract class NotificationBase : INotifyPropertyChanged
    {

        private readonly Queue<PropertyChangedEventArgs> _notifications = new Queue<PropertyChangedEventArgs>();
        private int _notificationSupressionDepth;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void InvokePropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, args);
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_notificationSupressionDepth == 0)
            {
                InvokePropertyChanged(e);
            }
            else
            {
                if (!_notifications.Contains(e, NotifyEventComparer.Instance))
                {
                    _notifications.Enqueue(e);
                }
            }
        }

        protected IDisposable QueueNotifications()
        {
            _notificationSupressionDepth++;
            return Disposable.Create(() =>
            {
                _notificationSupressionDepth--;
                TryNotify();
            });
        }

        protected IDisposable SupressNotifications()
        {
            _notificationSupressionDepth++;
            return Disposable.Create(() =>
            {
                _notificationSupressionDepth--;
            });
        }

        private void TryNotify()
        {
            if (_notificationSupressionDepth == 0)
            {
                while (_notifications.Count > 0)
                {
                    var notification = _notifications.Dequeue();
                    InvokePropertyChanged(notification);
                }
            }
        }
    }



    public sealed class NotifyEventComparer : IEqualityComparer<PropertyChangedEventArgs>
    {
        public static readonly NotifyEventComparer Instance = new NotifyEventComparer();

        bool IEqualityComparer<PropertyChangedEventArgs>.Equals(PropertyChangedEventArgs x, PropertyChangedEventArgs y)
        {
            return x.PropertyName == y.PropertyName;
        }

        int IEqualityComparer<PropertyChangedEventArgs>.GetHashCode(PropertyChangedEventArgs obj)
        {
            return obj.PropertyName.GetHashCode();
        }
    }
}