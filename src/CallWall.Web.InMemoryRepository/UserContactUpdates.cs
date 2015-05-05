using System;
using CallWall.Web.Domain;

namespace CallWall.Web.InMemoryRepository
{
    internal class UserContactUpdates
    {
        private readonly Guid _userId;
        private readonly IObservable<Event<ContactAggregateUpdate>> _contactUpdates;

        public UserContactUpdates(Guid userId, IObservable<Event<ContactAggregateUpdate>> contactUpdates)
        {
            _userId = userId;
            _contactUpdates = contactUpdates;
        }

        public Guid UserId { get { return _userId; } }

        public IObservable<Event<ContactAggregateUpdate>> ContactUpdates { get { return _contactUpdates; } }
    }
}