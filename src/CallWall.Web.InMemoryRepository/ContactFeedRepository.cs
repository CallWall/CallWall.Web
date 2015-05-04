using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.Providers;

namespace CallWall.Web.InMemoryRepository
{
    public class ContactFeedRepository : IContactFeedRepository, IRunnable
    {
        private readonly UserRepository _userRepository;
        private readonly ConcurrentDictionary<Guid, UserState> _userContactsMap = new ConcurrentDictionary<Guid, UserState>(); 
        private readonly IReadOnlyDictionary<string, IAccountContactProvider> _accountContactProviders;
        private readonly ILogger _logger;

        public ContactFeedRepository(
            ILoggerFactory loggerFactory,
            UserRepository userRepository, 
            IEnumerable<IAccountContactProvider> accountContactProviders)
        {
            _userRepository = userRepository;
            _logger = loggerFactory.CreateLogger(typeof(ContactFeedRepository));
            _accountContactProviders = accountContactProviders.ToReadOnlyDictionary(acp => acp.Provider);
        }

        public async Task Run()
        {
            _userRepository.AccountRefreshRequests.Subscribe(OnAccountContactRefresh);
            await Task.Yield();
        }

        public IObservable<int> ObserveContactUpdatesHeadVersion(User user)
        {
            _logger.Trace("ObserveContactUpdatesHeadVersion({0})", user.Id);
            return _userContactsMap[user.Id].HeadEventId;
        }

        public IObservable<Event<ContactAggregateUpdate>> GetContactUpdates(User user, int fromEventId)
        {
            _logger.Trace("GetContactUpdates({0}, {1})", user.Id, fromEventId);
            return _userContactsMap[user.Id].ContactUpdates.Where(evt => evt.EventId >= fromEventId);
        }

        private void OnAccountContactRefresh(AccountContactRefreshRequest refreshRequest)
        {
            _logger.Trace("OnAccountContactRefresh({0}, {1}, {2})", refreshRequest.UserId, refreshRequest.Account.AccountId, refreshRequest.TriggeredBy);
            var userState = _userContactsMap.GetOrAdd(
                refreshRequest.UserId,
                userId => new UserState(userId, _accountContactProviders));

            userState.RefreshAccount(refreshRequest.Account);
        }


        private sealed class UserState : IDisposable
        {
            private readonly Guid _userId;
            private readonly UserContacts _contactAggregator;
            private readonly IReadOnlyDictionary<string, IAccountContactProvider> _providers;
            private readonly ReplaySubject<Event<ContactAggregateUpdate>> _contactUpdates = new ReplaySubject<Event<ContactAggregateUpdate>>();
            private readonly BehaviorSubject<int> _head = new BehaviorSubject<int>(-1);
            private readonly ConcurrentDictionary<string, DateTime> _accountTimeStamp = new ConcurrentDictionary<string, DateTime>();
            private readonly CompositeDisposable _contactFeedSubscription = new CompositeDisposable();

            public UserState(Guid userId, IReadOnlyDictionary<string, IAccountContactProvider> accountContactProviders)
            {
                _userId = userId;
                _contactAggregator = new UserContacts(_userId);
                _providers = accountContactProviders;
                _contactUpdates.Select(evt => evt.EventId)
                    .Subscribe(_head);
            }

            public IObservable<Event<ContactAggregateUpdate>> ContactUpdates
            {
                get { return _contactUpdates.AsObservable(); }
            }

            public BehaviorSubject<int> HeadEventId
            {
                get { return _head; }
            }

            public void RefreshAccount(IAccount account)
            {
                var provider = _providers[account.Provider];
                var lastUpdate = _accountTimeStamp.GetOrAdd(account.AccountId, DateTime.MinValue);

                var subscription = provider.GetContactsFeed(account, lastUpdate)
                    .Buffer(TimeSpan.FromSeconds(1))
                    //TODO: Serialize on to a single thread here? -LC
                    .SelectMany(buffer =>
                    {
                        var userContacts = _contactAggregator;
                        using (userContacts.TrackChanges())
                        {
                            foreach (var accountContactSummary in buffer)
                            {
                                userContacts.Add(accountContactSummary);
                            }
                            var snapshot = userContacts.GetChangesSnapshot();
                            userContacts.CommitChanges();
                            return snapshot;
                        }
                    })
                    .Select((cau, i) => new Event<ContactAggregateUpdate>(i, cau))
                    .Subscribe(
                        contactAggregateUpdate => _contactUpdates.OnNext(contactAggregateUpdate),
                        () => _accountTimeStamp.TryUpdate(account.AccountId, DateTime.Now, lastUpdate));

                _contactFeedSubscription.Add(subscription);
            }

            public void Dispose()
            {
                _contactFeedSubscription.Dispose();
            }
        }

    }
}