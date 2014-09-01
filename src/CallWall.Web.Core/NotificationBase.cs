using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace CallWall.Web
{
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
}