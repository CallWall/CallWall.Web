using System;
using System.Reactive.Linq;
using CallWall.Web.EventStore.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class UserContactRepository : IUserContactRepository
    {
        private readonly EventStore _eventStore;

        public UserContactRepository(IEventStoreConnectionFactory connectionFactory)
        {
            _eventStore = new EventStore(connectionFactory);
        }

        public IObservable<ContactAggregateUpdate> GetContactSummariesFrom(User user, int? versionId)
        {
            var streamName = ContactStreamNames.UserContacts(user.Id);
            return _eventStore.GetEvents(streamName, versionId)
                .SelectMany(resolvedEvent => resolvedEvent.OriginalEvent.Deserialize<ContactAggregateUpdate[]>());
        }
    }
}