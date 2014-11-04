using System;
using CallWall.Web.Domain;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubAccountContactProvider : IAccountContactProvider
    {
        private readonly string _provider;
        private readonly IObservable<IAccountContactSummary> _contactFeed;

        public StubAccountContactProvider(string provider, IObservable<IAccountContactSummary> contactFeed)
        {
            _provider = provider;
            _contactFeed = contactFeed;
        }

        public string Provider { get { return _provider; } }

        public IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            return _contactFeed;
        }

        IObservable<IContactProfile> IAccountContactProvider.GetContactDetails(User user, string[] contactKeys)
        {
            throw new NotSupportedException();
        }
    }
}