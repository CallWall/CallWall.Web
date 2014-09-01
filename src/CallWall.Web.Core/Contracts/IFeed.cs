using System;

namespace CallWall.Web.Contracts
{
    public interface IFeed<T>
    {
        int TotalResults { get; }
        IObservable<IAccountContactSummary> Values { get; }
    }
}