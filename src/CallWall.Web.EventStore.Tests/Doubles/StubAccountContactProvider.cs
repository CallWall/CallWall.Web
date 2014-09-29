using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubAccountContactProvider : IAccountContactProvider
    {
        private readonly string _provider;
        private readonly IFeed<IAccountContactSummary> _feed;

        public StubAccountContactProvider(string provider, IObservable<IAccountContactSummary> contactFeed, int totalExpectedContacts)
        {
            _provider = provider;
            _feed = new StubFeed(totalExpectedContacts, contactFeed);
        }

        public string Provider { get { return _provider; } }

        public IObservable<IFeed<IAccountContactSummary>> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            return Observable.Return(_feed);
        }

        IObservable<IContactProfile> IAccountContactProvider.GetContactDetails(IEnumerable<ISession> session, string[] contactKeys)
        {
            throw new NotSupportedException();
        }
    }
}