using System;
using System.ComponentModel;

namespace CallWall.Web.EventStore
{
    public interface IDomainEventBase : INotifyPropertyChanged, IRunnable, IDisposable
    {
        DomainEventState State { get; }
    }
}