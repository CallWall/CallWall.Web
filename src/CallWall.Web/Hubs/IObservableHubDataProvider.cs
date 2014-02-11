using System;

namespace CallWall.Web.Hubs
{
    public interface IObservableHubDataProvider<out T>
    {
        IObservable<T> GetObservable();//This will need to pass in a param so we know who to get info for
    }
}