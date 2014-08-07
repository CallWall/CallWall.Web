using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Tests.Doubles;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace CallWall.Web.EventStore.Tests
{
    public class UserRepository : IDisposable //: IUserRepository
    {
        private const string UserStreamName = "Users";
        private readonly EventStore _eventStore;
        private int _writeVersion = ExpectedVersion.NoStream;
        private int _readVersion = ExpectedVersion.NoStream;
        private IDisposable _eventSubscription;
        private IConnectableObservable<bool> _isUpToDate;

        private readonly List<User> _userCache = new List<User>();

        public UserRepository(IEventStoreConnectionFactory connectionFactory)
        {
            _eventStore = new EventStore(connectionFactory);
        }

        public IObservable<bool> IsUpToDate { get { return _isUpToDate; } }

        public async Task Load()
        {
            _writeVersion = await _eventStore.GetHeadVersion(UserStreamName);

            var sharedUserEvents = _eventStore.GetEvents(UserStreamName)
                .Publish();
            _isUpToDate = sharedUserEvents.Select(ev => ev.OriginalEventNumber == _writeVersion)
                .StartWith(false)
                .Replay(1);
            //Load into memory the previous events
            var eventSubscription = sharedUserEvents
                .Subscribe(
                    OnUserEvent,
                    OnError);
            
            _eventSubscription = new CompositeDisposable(eventSubscription, _isUpToDate.Connect(),
                sharedUserEvents.Connect());
        }

        private void OnUserEvent(ResolvedEvent userEvent)
        {
            _readVersion = userEvent.OriginalEventNumber;

            switch (userEvent.OriginalEvent.EventType)
            {
                case UserEventType.UserCreated:
                    AddUser(userEvent.OriginalEvent.Data);
                    break;
            }
        }

        private void AddUser(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            var userCreatedEvent = JsonConvert.DeserializeObject<UserCreatedEvent>(json);
            var accounts = userCreatedEvent.Accounts.Select(a => new Account()
            {
                AccountId = a.AccountId,
                Provider = a.Provider,
                DisplayName = a.DisplayName,
                CurrentSession = new Session(
                    a.CurrentSession.AccessToken, 
                    a.CurrentSession.RefreshToken, 
                    a.CurrentSession.Expires,
                    a.CurrentSession.AuthorizedResources)
            });

            var user = new User(userCreatedEvent.DisplayName, accounts);
            _userCache.Add(user);
        }

        private void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public async Task<User> RegisterNewUser(IAccount account, Guid eventId)
        {
            Guard.ArgumentNotNull(account, "account");
            if (eventId == Guid.Empty) throw new ArgumentException("Must provide a non-zero eventId", "eventId");

            return await CreateUserStream(account, eventId);
        }

        private async Task<User> CreateUserStream(IAccount account, Guid eventId)
        {
            var userCreatedEvent = new UserCreatedEvent
            {
                DisplayName = account.DisplayName,
                Accounts =
                {
                    new AccountRecord
                    {
                        Provider = account.Provider,
                        AccountId = account.AccountId,
                        DisplayName = account.DisplayName,
                        CurrentSession = new SessionRecord
                        {
                            AccessToken =  account.CurrentSession.AccessToken,
                            RefreshToken = account.CurrentSession.RefreshToken,
                            Expires = account.CurrentSession.Expires,
                            AuthorizedResources = account.CurrentSession.AuthorizedResources.ToArray()
                        }
                    }
                }
            }.ToJson();

            await _eventStore.SaveEvent(UserStreamName,
                _writeVersion,
                eventId,
                UserEventType.UserCreated,
                userCreatedEvent);

            _writeVersion++;

            return new User(account.DisplayName, new[] {account});
        }

        public User FindByAccount(IAccount account)
        {
            return _userCache.FirstOrDefault(u => u.Accounts.Any(a =>
                a.Provider == account.Provider
                &&
                a.AccountId == account.AccountId));
        }






        private static class AccountEventType
        {
            public const string AccountRegististered = "AccountRegistered";

            /// <summary>
            /// Indicates that this Account was used to login to CallWall.
            /// </summary>
            public static readonly string AccountLogin = "AccountLogin";

            /// <summary>
            /// Indicates that the user has removed this account from CallWall
            /// </summary>
            public static readonly string AccountDeregistered = "AccountDeregistered";

            /// <summary>
            /// Indicates that the Provider or User has revoked access from CallWall accessing this account.
            /// </summary>
            public static readonly string AccountRevoked = "AccountRevoked";
                                          //May have to be a linked event from a AccountContactSummaryRefresh failure. -LC
        }

        private static class UserEventType
        {
            public const string UserCreated = "UserCreated";
        }

        public class UserCreatedEvent
        {
            public UserCreatedEvent()
            {
                Accounts = new List<AccountRecord>();
            }

            public string DisplayName { get; set; }
            public List<AccountRecord> Accounts { get; set; }
        }

        public void Dispose()
        {
            if(_eventSubscription!=null)
                _eventSubscription.Dispose();
        }
    }

    public class Account : IAccount
    {
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public ISession CurrentSession { get; set; }

        private bool Equals(IAccount other)
        {
            return string.Equals(Provider, other.Provider)
                && string.Equals(AccountId, other.AccountId)
                && string.Equals(DisplayName, other.DisplayName)
                && Equals(CurrentSession, other.CurrentSession);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as IAccount;
            if (other == null) return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Provider != null ? Provider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AccountId != null ? AccountId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CurrentSession != null ? CurrentSession.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Account left, IAccount right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Account left, IAccount right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("Provider: {0}, AccountId: {1}, DisplayName: {2}, CurrentSession: {3}", Provider, AccountId, DisplayName, CurrentSession);
        }
    }

    public class AccountRecord 
    {
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public SessionRecord CurrentSession { get; set; }

        public override string ToString()
        {
            return string.Format("Provider: {0}, AccountId: {1}, DisplayName: {2}, CurrentSession: {3}", Provider, AccountId, DisplayName, CurrentSession);
        }
    }

    public class SessionRecord
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset Expires { get; set; }
        public string[] AuthorizedResources { get; set; }
    }
}