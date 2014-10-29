using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class UserContactRepository : IUserContactRepository
    {
        private readonly IEventStoreClient _eventStoreClient;

        public UserContactRepository(IEventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        public IObservable<Event<ContactAggregateUpdate>> GetContactSummariesFrom(User user, int? versionId)
        {
            var streamName = ContactStreamNames.UserContacts(user.Id);
            return _eventStoreClient.GetEvents(streamName, versionId)
                .Where(resolvedEvent => resolvedEvent.OriginalEvent!=null)
                .Select(resolvedEvent => new Event<ContactAggregateUpdate>(resolvedEvent.OriginalEventNumber, resolvedEvent.OriginalEvent.Deserialize<ContactAggregateUpdate>()));
        }

        public IObservable<int> ObserveContactUpdatesHeadVersion(User user)
        {
            var streamName = ContactStreamNames.UserContacts(user.Id);

            return Observable.Concat(
                _eventStoreClient.GetHeadVersion(streamName).ToObservable(),
                _eventStoreClient.GetNewEvents(streamName).Select(resolvedEvent => resolvedEvent.OriginalEventNumber));

        }
    }
}