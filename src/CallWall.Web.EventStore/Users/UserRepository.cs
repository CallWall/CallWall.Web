using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Accounts;
using CallWall.Web.EventStore.Contacts;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;

namespace CallWall.Web.EventStore.Users
{
    public class UserRepository : DomainEventBase, IUserRepository
    {
        private readonly IAccountFactory _accountFactory;
        private readonly IAccountContactRefresher _accountContactRefresher;
        //TODO: Current model for in-mem cache of Users wont scale. Fine while in POC -LC
        private readonly List<User> _userCache = new List<User>();

        public UserRepository(IEventStoreClient eventStoreClient, ILoggerFactory loggerFactory, IAccountFactory accountFactory, IAccountContactRefresher accountContactRefresher)
            : base(eventStoreClient, loggerFactory, "Users")
        {
            _accountFactory = accountFactory;
            _accountContactRefresher = accountContactRefresher;
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
                await RequestAccountRefresh(user, ContactRefreshTriggers.Login);
            }

            return user;
        }

        public async Task<User> RegisterAccount(Guid userId, IAccount account)
        {
            await AddAccountToUser(userId, account, Guid.NewGuid());
            await _accountContactRefresher.RequestRefresh(userId, account, ContactRefreshTriggers.Registered);
            return await FindByAccount(account);
        }

        public async Task<User> GetUserBy(Guid userId)
        {
            await WaitWhileBehind();
            var user = _userCache.FirstOrDefault(u => u.Id == userId);
            return user;
        }


        protected override void OnEventReceived(ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.OriginalEvent.EventType)
            {
                case UserEventType.UserCreated:
                    AddUserToCache(resolvedEvent.OriginalEvent);
                    break;
                case UserEventType.AccountRegistered:
                    AddAccountToCache(resolvedEvent.OriginalEvent);
                    break;
            }
        }

        protected override void OnStreamError(Exception error)
        {
            Logger.Error(error, "Sequence failed.");
            throw new NotImplementedException();
        }


        private void AddUserToCache(RecordedEvent recordedEvent)
        {
            var userCreatedEvent = recordedEvent.Deserialize<UserCreatedEvent>();
            var account = CreateAccount(userCreatedEvent.Account);
            var accounts = new[] { account };

            var user = new User(userCreatedEvent.Id, userCreatedEvent.DisplayName, accounts);
            _userCache.Add(user);
        }

        private void AddAccountToCache(RecordedEvent recordedEvent)
        {
            var userCreatedEvent = recordedEvent.Deserialize<UserRegisteredAccountEvent>();
            var account = CreateAccount(userCreatedEvent.Account);

            var useridx = _userCache.FindIndex(u=>u.Id == userCreatedEvent.UserId);
            var cachedUser = _userCache[useridx];
            var newUser = cachedUser.AddAccount(account);
            _userCache[useridx] = newUser;
        }

        private async Task<User> RegisterNewUser(IAccount account, Guid eventId)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (eventId == Guid.Empty) throw new ArgumentException("Must provide a non-zero eventId", "eventId");

            var user = await CreateUserStream(account, eventId);
            await CreateUserContactStream(user.Id);
            await RequestAccountRefresh(user, ContactRefreshTriggers.Registered);
            return user;
        }

        private async Task<User> CreateUserStream(IAccount account, Guid eventId)
        {
            Logger.Debug("Creating a user stream for new Account {0}", account.AccountId);
            
            var userCreatedEvent = new UserCreatedEvent
            {
                Id = Guid.NewGuid(),
                DisplayName = account.DisplayName,
                Account = new AccountRecord(account)
            };
            var payload = userCreatedEvent.ToJson();

            await WriteEvent(eventId, UserEventType.UserCreated, payload);

            return new User(userCreatedEvent.Id, userCreatedEvent.DisplayName, new[] { account });
        }

        private async Task AddAccountToUser(Guid userId, IAccount account, Guid eventId)
        {
            var userCreatedEvent = new UserRegisteredAccountEvent
            {
                UserId = userId,
                Account = new AccountRecord(account)
            };
            var payload = userCreatedEvent.ToJson();

            await WriteEvent(eventId, UserEventType.AccountRegistered, payload);
        }
        
        private async Task CreateUserContactStream(Guid id)
        {
            Logger.Debug("Creating UserContactStream with empty initial payload");
            var streamName = ContactStreamNames.UserContacts(id);
            await EventStoreClient.SaveEvent(streamName, ExpectedVersion.NoStream, Guid.NewGuid(), "CreatingUserContactStream",
                string.Empty);
        }
        
        private async Task RequestAccountRefresh(User user, ContactRefreshTriggers triggeredBy)
        {
            Logger.Trace("Requesting account refresh for userId {0}", user.Id);
            await Task.WhenAll(
                user.Accounts.Select(account =>
                    _accountContactRefresher.RequestRefresh(user.Id, account, triggeredBy)
                ));
        }

        private async Task<User> FindByAccount(IAccount account)
        {
            await WaitWhileBehind();

            var user = _userCache.FirstOrDefault(u => u.Accounts.Any(a =>
                a.Provider == account.Provider
                &&
                a.AccountId == account.AccountId));

            return user;
        }

        private async Task WaitWhileBehind()
        {
            Logger.Trace("State : {0}", State);
            if (!(State.IsListening && !State.IsProcessing))
            {
                Logger.Trace("Waiting for state to change...");
            }
            await this.PropertyChanges(ur => ur.State)
                .StartWith(State)
                .Where(s => s.IsListening && !s.IsProcessing)
                .Take(1)
                .ToTask();
        }

        private IAccount CreateAccount(AccountRecord accountRecord)
        {
            return _accountFactory.Create(accountRecord.AccountId,
                accountRecord.Provider,
                accountRecord.DisplayName,
                accountRecord.CurrentSession,
                ((IAccount)accountRecord).Handles);
        }
    }
}
