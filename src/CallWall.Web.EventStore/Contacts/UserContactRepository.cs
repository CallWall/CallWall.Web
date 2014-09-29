using System;
using System.Reactive.Linq;

namespace CallWall.Web.EventStore.Contacts
{
    public class UserContactRepository : IUserContactRepository
    {
        private readonly IEventStoreClient _eventStoreClient;

        public UserContactRepository(IEventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        public IObservable<ContactAggregateUpdate> GetContactSummariesFrom(User user, int? versionId)
        {
            var streamName = ContactStreamNames.UserContacts(user.Id);
            return _eventStoreClient.GetEvents(streamName, versionId)
                .Where(resolvedEvent => resolvedEvent.OriginalEvent!=null)
                .Select(resolvedEvent => resolvedEvent.OriginalEvent.Deserialize<ContactAggregateUpdate>());
        }
    }
}