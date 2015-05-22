using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class EventStoreContactFeedRepository : IContactFeedRepository, IContactRepository
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ILogger _logger;

        public EventStoreContactFeedRepository(IEventStoreClient eventStoreClient, ILoggerFactory loggerFactory)
        {
            _eventStoreClient = eventStoreClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }
        
        public IObservable<int> ObserveContactUpdatesHeadVersion(User user)
        {
            var streamName = ContactStreamNames.UserContacts(user.Id);

            return Observable.Concat(
                _eventStoreClient.GetHeadVersion(streamName).ToObservable(),
                _eventStoreClient.GetNewEvents(streamName).Select(resolvedEvent => resolvedEvent.OriginalEventNumber));
        }

        public IObservable<Event<ContactAggregateUpdate>> GetContactUpdates(User user, int versionId)
        {
            var streamName = ContactStreamNames.UserContacts(user.Id);
            return _eventStoreClient.GetEvents(streamName, versionId)
                .Where(resolvedEvent => resolvedEvent.OriginalEvent != null)
                .Select(resolvedEvent => new Event<ContactAggregateUpdate>(resolvedEvent.OriginalEventNumber, resolvedEvent.OriginalEvent.Deserialize<ContactAggregateUpdate>()));
        }

        public IObservable<IContactProfile> GetContactDetails(User user, string contactId)
        {
            return GetContactLookupFor(user).Select(cl => cl.GetById(int.Parse(contactId)));
        }

        public IObservable<IContactProfile> LookupContactByHandles(User user, ContactHandle[] contactHandles)
        {
            var keys = contactHandles.SelectMany(ch => ch.NormalizedHandle())
                .ToArray();
            return GetContactLookupFor(user)
                .Log(_logger, "GetContactDetails")
                .SelectMany(cl => cl.GetByContactKeys(keys));
        }

        private IObservable<ContactLookup> GetContactLookupFor(User user)
        {
            var streamName = ContactStreamNames.UserContacts(user.Id);
            var query =
                from headVer in _eventStoreClient.GetHeadVersion(streamName).ToObservable()
                        .Log(_logger, "UserContact-Head")
                from contactUpdate in _eventStoreClient.GetEvents(streamName)
                    .Where(resolvedEvent => resolvedEvent.OriginalEvent != null)
                    .TakeUntil(re => re.OriginalEventNumber == headVer)
                    .Select(resolvedEvent => resolvedEvent.OriginalEvent.Deserialize<ContactAggregateUpdate>())
                    .Where(x => x != null)
                    .Log(_logger, "UserContact-Profile")
                select contactUpdate;
            return query.Aggregate(new ContactLookup(), (acc, cur) => acc.Add(cur))
                        .Log(_logger, "UserContact-Aggregate");
        }
    }
}