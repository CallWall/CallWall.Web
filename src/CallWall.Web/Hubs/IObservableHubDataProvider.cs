using System;

namespace CallWall.Web.Hubs
{
    public interface IObservableHubDataProvider<out T>
    {
        IObservable<T> GetObservable();
    }
}