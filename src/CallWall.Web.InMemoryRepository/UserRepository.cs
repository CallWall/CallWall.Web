using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.InMemoryRepository
{
    public class UserRepository : NotificationBase, IUserRepository
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Guid, User> _userCache = new ConcurrentDictionary<Guid, User>();
        private readonly Subject<AccountContactRefreshRequest> _accountRefreshRequests = new Subject<AccountContactRefreshRequest>();

        public UserRepository(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        private ILogger Logger { get { return _logger; } }
        
        Task IRunnable.Run()
        {
            return new Task(() => { });
        }

        public async Task<User> Login(IAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            Logger.Debug("Finding User by account '{0}'", account.AccountId);
            var user = await FindByAccount(account);
            if (user == null)
            {
                Logger.Debug("User not found for account '{0}'. Registering as new user", account.AccountId);
                user = await RegisterNewUser(account, Guid.NewGuid());
                Logger.Debug("User registered for account - UserId:{0}, Accounts:{1}", user.Id, string.Join(",", user.Accounts.Select(a => a.AccountId)));
            }
            else
            {
                Logger.Debug("User found for account - UserId:{0}, Accounts:{1}", user.Id, string.Join(",", user.Accounts.Select(a => a.AccountId)));
                Logger.Trace("Requesting account refresh for userId {0}", user.Id);
                await RequestAccountRefreshForAllUserAccounts(user, ContactRefreshTriggers.Login);
            }

            return user;
        }

        public async Task<User> RegisterAccount(Guid userId, IAccount account)
        {
            Logger.Debug("Registering AccountId:'{0}' to UserId:'{1}'", account.AccountId, userId);
            var currentUser = _userCache[userId];
            var newUser = currentUser.AddAccount(account);
            _userCache[userId] = newUser;
            RequestAccountRefresh(userId, ContactRefreshTriggers.Registered, account);
            return await Task.FromResult(newUser);
        }

        public Task<User> GetUserBy(Guid userId)
        {
            return Task.Factory.StartNew(() => _userCache[userId]);
        }

        public IObservable<AccountContactRefreshRequest> AccountRefreshRequests
        {
            get { return _accountRefreshRequests.AsObservable(); }
        }
        
        private async Task<User> FindByAccount(IAccount account)
        {
            return await Task.Run(() => _userCache
                .Select(kvp => kvp.Value)
                .FirstOrDefault(u => u.Accounts.Any(a =>
                    a.Provider == account.Provider
                    &&
                    a.AccountId == account.AccountId)));
        }

        private async Task<User> RegisterNewUser(IAccount account, Guid eventId)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (eventId == Guid.Empty) throw new ArgumentException("Must provide a non-zero eventId", "eventId");

            var user = CreateUser(account);
            _userCache.AddOrUpdate(user.Id,
                user,
                (id, prev) => prev.AddAccount(account));
            await RequestAccountRefreshForAllUserAccounts(user, ContactRefreshTriggers.Registered);
            return user;
        }

        private async Task RequestAccountRefreshForAllUserAccounts(User user, ContactRefreshTriggers triggeredBy)
        {
            Logger.Trace("Requesting account refresh for userId {0}", user.Id);
            await Task.Factory.StartNew(() =>
                {
                    foreach (var acc in user.Accounts)
                    {
                        RequestAccountRefresh(user.Id, triggeredBy, acc);
                    }
                });
        }

        private void RequestAccountRefresh(Guid userId, ContactRefreshTriggers triggeredBy, IAccount acc)
        {
            Logger.Trace("Requesting account refresh for userId:'{0}', AccountId:'{1}'", userId, acc.AccountId);
            var refreshRequest = new AccountContactRefreshRequest(userId, acc, triggeredBy);
            _accountRefreshRequests.OnNext(refreshRequest);
        }

        private static User CreateUser(IAccount account)
        {
            return new User(Guid.NewGuid(), account.DisplayName, new[] { account });
        }

        public void Dispose()
        {
            _userCache.Clear();
            _accountRefreshRequests.Dispose();
        }
    }

    public class AccountContactRefreshRequest
    {
        private readonly Guid _userId;
        private readonly IAccount _account;
        private readonly ContactRefreshTriggers _triggeredBy;

        public AccountContactRefreshRequest(Guid userId, IAccount account, ContactRefreshTriggers triggeredBy)
        {
            _userId = userId;
            _account = account;
            _triggeredBy = triggeredBy;
        }

        public Guid UserId { get { return _userId; } }

        public IAccount Account { get { return _account; } }

        public ContactRefreshTriggers TriggeredBy { get { return _triggeredBy; } }
    }
}