using System;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubFeed : IFeed<IAccountContactSummary>
    {
        private readonly int _totalResults;
        private readonly IObservable<IAccountContactSummary> _values;

        public StubFeed(int totalResults, IObservable<IAccountContactSummary> values)
        {
            _totalResults = totalResults;
            _values = values;
        }

        public int TotalResults { get { return _totalResults; } }

        public IObservable<IAccountContactSummary> Values { get { return _values; } }
    }
}