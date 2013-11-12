using System;

namespace CallWall.Web
{
    public interface IFeed<T>
    {
        int TotalResults { get; }
        IObservable<IContactSummary> Values { get; }
    }
}