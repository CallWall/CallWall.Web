using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.Providers;
using EventStore.ClientAPI;

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
                .Where(resolvedEvent => resolvedEvent.OriginalEvent != null)
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

    public class EventStoreAccountContactProvider : IAccountContactProvider
    {
        private readonly IEventStoreClient _eventStoreClient;
        private ILogger _logger;

        public EventStoreAccountContactProvider(IEventStoreClient eventStoreClient, ILoggerFactory loggerFactory)
        {
            _eventStoreClient = eventStoreClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public string Provider { get { return ""; } }

        public IObservable<IAccountContactSummary> GetContactsFeed(IAccount account, DateTime lastUpdated)
        {
            return Observable.Empty<IAccountContactSummary>();
        }

        public IObservable<IContactProfile> GetContactDetails(User user, string[] contactKeys)
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
                .Log(_logger, "UserContact-Aggregate")
                .Select(cl => cl.GetByContactKeys(contactKeys));
        }
    }

    public static class ObservableEx
    {
        public static IObservable<T> TakeUntil<T>(this IObservable<T> source, Func<T, bool> terminator)
        {
            return Observable.Create<T>(o =>
                source.Subscribe(x =>
                {
                    o.OnNext(x);
                    if (terminator(x))
                        o.OnCompleted();
                },
                o.OnError,
                o.OnCompleted));
        }
    }
}