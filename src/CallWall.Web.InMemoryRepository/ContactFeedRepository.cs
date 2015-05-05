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
    sealed class ContactFeedRepository : IContactFeedRepository, IRunnable
    {
        private readonly object _gate = new object();
        private readonly UserRepository _userRepository;
        private readonly Dictionary<Guid, UserState> _userContactsMap = new Dictionary<Guid, UserState>(); 
        private readonly Subject<UserState> _allUserContactUpdates = new Subject<UserState>(); 
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
            await Task.FromResult(0);
        }

        internal IObservable<UserContactUpdates> GetAllUserContactUpdates()
        {
            return _allUserContactUpdates.Select(us => new UserContactUpdates(us.UserId, us.ContactUpdates));
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

            UserState userState;
            var userId = refreshRequest.UserId;
            lock (_gate)
            {
                if (!_userContactsMap.TryGetValue(userId, out userState))
                {
                    userState = new UserState(userId, _accountContactProviders);
                    _userContactsMap[userId] = userState;
                    _allUserContactUpdates.OnNext(userState);
                }
            }
            

            userState.RefreshAccount(refreshRequest.Account);
        }

        private sealed class UserState : IDisposable
        {
            private readonly UserContacts _contactAggregator;
            private readonly Guid _userId;
            private readonly IReadOnlyDictionary<string, IAccountContactProvider> _providers;
            private readonly ReplaySubject<Event<ContactAggregateUpdate>> _contactUpdates = new ReplaySubject<Event<ContactAggregateUpdate>>();
            private readonly BehaviorSubject<int> _head = new BehaviorSubject<int>(-1);
            private readonly ConcurrentDictionary<string, DateTime> _accountTimeStamp = new ConcurrentDictionary<string, DateTime>();
            private readonly CompositeDisposable _contactFeedSubscription = new CompositeDisposable();

            public UserState(Guid userId, IReadOnlyDictionary<string, IAccountContactProvider> accountContactProviders)
            {
                _userId = userId; 
                _contactAggregator = new UserContacts(userId);
                _providers = accountContactProviders;
                _contactUpdates.Select(evt => evt.EventId)
                    .Subscribe(_head);
            }

            public Guid UserId { get { return _userId; } }

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