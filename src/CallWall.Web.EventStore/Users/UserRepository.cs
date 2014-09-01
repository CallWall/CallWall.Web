using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Accounts;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;

namespace CallWall.Web.EventStore.Users
{
    public class UserRepository : DomainEventBase, IUserRepository
    {
        private readonly IAccountContactRefresher _accountContactRefresher;
        private readonly List<User> _userCache = new List<User>();

        public UserRepository(IEventStoreConnectionFactory connectionFactory, IAccountContactRefresher accountContactRefresher)
            : base(connectionFactory, "Users")
        {
            _accountContactRefresher = accountContactRefresher;
        }

        public async Task<User> FindByAccount(IAccount account)
        {
            await this.PropertyChanges(ur => ur.State)
                .StartWith(State)
                .Where(s => s.IsListening && !s.IsProcessing)
                .Take(1)
                .ToTask();

            var user = _userCache.FirstOrDefault(u => u.Accounts.Any(a =>
                a.Provider == account.Provider
                &&
                a.AccountId == account.AccountId));

            return user;
        }

        public async Task<User> RegisterNewUser(IAccount account, Guid eventId)
        {
            if(account==null) throw new ArgumentNullException("account");
            if (eventId == Guid.Empty) throw new ArgumentException("Must provide a non-zero eventId", "eventId");

            var user = await CreateUserStream(account, eventId);
            await account.RefreshContacts(user.Id, ContactRefreshTriggers.Registered);
            return user;
        }

        protected override void OnEventReceived(ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.OriginalEvent.EventType)
            {
                case UserEventType.UserCreated:
                    AddUser(resolvedEvent.OriginalEvent);
                    break;
            }
        }

        protected override void OnStreamError(Exception error)
        {
            throw new NotImplementedException();
        }

        private void AddUser(RecordedEvent recordedEvent)
        {
            var userCreatedEvent = recordedEvent.Deserialize<UserCreatedEvent>();
            var account = CreateAccount(userCreatedEvent.Account);
            var accounts = new[] { account };

            var user = new User(userCreatedEvent.Id, userCreatedEvent.DisplayName, accounts);
            _userCache.Add(user);
        }

        private Account CreateAccount(AccountRecord accountRecord)
        {
            return new Account(this, _accountContactRefresher)
            {
                AccountId = accountRecord.AccountId,
                Provider = accountRecord.Provider,
                DisplayName = accountRecord.DisplayName,
                CurrentSession = new Session(
                    accountRecord.CurrentSession.AccessToken,
                    accountRecord.CurrentSession.RefreshToken,
                    accountRecord.CurrentSession.Expires,
                    accountRecord.CurrentSession.AuthorizedResources)
            };
        }

        private async Task<User> CreateUserStream(IAccount account, Guid eventId)
        {
            var userCreatedEvent = new UserCreatedEvent
            {
                Id = Guid.NewGuid(),
                DisplayName = account.DisplayName,
                Account =
                    new AccountRecord
                    {
                        Provider = account.Provider,
                        AccountId = account.AccountId,
                        DisplayName = account.DisplayName,
                        CurrentSession = new SessionRecord(account.CurrentSession)
                    }
            };
            var payload = userCreatedEvent.ToJson();

            await WriteEvent(eventId, UserEventType.UserCreated, payload);

            return new User(userCreatedEvent.Id, userCreatedEvent.DisplayName, new[] { account });
        }
    }
}
