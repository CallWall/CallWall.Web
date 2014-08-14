using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
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
using CallWall.Web.EventStore.Domain;
using CallWall.Web.EventStore.Tests.Doubles;
using CallWall.Web.Providers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace CallWall.Web.EventStore.Tests
{
    public interface IUserRepository : IDomainEventBase
    {
        Task<User> FindByAccount(IAccount account);
        Task<User> RegisterNewUser(IAccount account, Guid eventId);
    }

    public interface IAccountContactsFactory
    {
        IAccountContacts Create(string provider, string accountId);
    }

    public class UserRepository : DomainEventBase, IUserRepository
    {
        private readonly IAccountContactsFactory _accountContactsFactory;

        private readonly List<User> _userCache = new List<User>();

        public UserRepository(IEventStoreConnectionFactory connectionFactory, IAccountContactsFactory accountContactsFactory )
            : base(connectionFactory, "Users")
        {
            _accountContactsFactory = accountContactsFactory;
        }
        
        public async Task<User> FindByAccount(IAccount account)
        {
            await this.PropertyChanges(ur => ur.State)
                .StartWith(State)
                .Where(s => s.IsListening && !s.IsProcessing)
                .Take(1)
                .ToTask();

            var user =  _userCache.FirstOrDefault(u => u.Accounts.Any(a =>
                a.Provider == account.Provider
                &&
                a.AccountId == account.AccountId));

            return user;
        }

        protected override void OnEventReceived(ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.OriginalEvent.EventType)
            {
                case UserEventType.UserCreated:
                    AddUser(resolvedEvent.OriginalEvent.Data);
                    break;
            }
        }

        private void AddUser(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            var userCreatedEvent = JsonConvert.DeserializeObject<UserCreatedEvent>(json);
            var accounts = userCreatedEvent.Accounts.Select(CreateAccount);

            var user = new User(userCreatedEvent.DisplayName, accounts);
            _userCache.Add(user);
        }

        private Account CreateAccount(AccountRecord accountRecord)
        {
            var accountContacts = _accountContactsFactory.Create(accountRecord.Provider, accountRecord.AccountId);
            return new Account(this, accountContacts)
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

        protected override void OnStreamError(Exception error)
        {
            throw new NotImplementedException();
        }

        public async Task<User> RegisterNewUser(IAccount account, Guid eventId)
        {
            Guard.ArgumentNotNull(account, "account");
            if (eventId == Guid.Empty) throw new ArgumentException("Must provide a non-zero eventId", "eventId");

            var user = await CreateUserStream(account, eventId);
            account.RefreshContacts();
            return user;
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

            await WriteEvent(eventId, UserEventType.UserCreated, userCreatedEvent);
            
            return new User(account.DisplayName, new[] {account});
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
    }

    public class Account : IAccount
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountContacts _accountContacts;

        public Account(IUserRepository userRepository, IAccountContacts accountContacts)
        {
            Guard.ArgumentNotNull(userRepository, "userRepository");
            Guard.ArgumentNotNull(accountContacts, "accountContacts");
            _userRepository = userRepository;
            _accountContacts = accountContacts;
        }

        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public ISession CurrentSession { get; set; }

        public async Task<User> Login()
        {
            var user = await _userRepository.FindByAccount(this);
            foreach (var account in user.Accounts)
            {
                account.RefreshContacts();
            }
            return user;
        }

        public void RefreshContacts()
        {
            _accountContacts.RequestRefresh();
        }


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

    public static class NotificationExtensions
    {
        /// <summary>
        /// Returns an observable sequence of a property value when the source raises <seealso cref="INotifyPropertyChanged.PropertyChanged"/> for the given property.
        /// </summary>
        /// <typeparam name="T">The type of the source object. Type must implement <seealso cref="INotifyPropertyChanged"/>.</typeparam>
        /// <typeparam name="TProperty">The type of the property that is being observed.</typeparam>
        /// <param name="source">The object to observe property changes on.</param>
        /// <param name="property">An expression that describes which property to observe.</param>
        /// <returns>Returns an observable sequence of property values when the property changes.</returns>
        public static IObservable<TProperty> PropertyChanges<T, TProperty>(this T source, Expression<Func<T, TProperty>> property)
            where T : class, INotifyPropertyChanged
        {
            if (source == null) throw new ArgumentNullException("source");

            var propertyName = property.GetPropertyInfo().Name;
            var propertySelector = property.Compile();

            return Observable.Create<TProperty>(
                o => Observable.FromEventPattern
                         <PropertyChangedEventHandler, PropertyChangedEventArgs>
                         (
                             h => source.PropertyChanged += h,
                             h => source.PropertyChanged -= h
                         )
                         .Where(e => e.EventArgs.PropertyName == propertyName)// || string.IsNullOrEmpty(e.EventArgs.PropertyName))
                         .Select(e => propertySelector(source))
                         .Subscribe(o));
        }

        /// <summary>
        /// Returns an observable sequence when <paramref name="source"/> raises <seealso cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source object. Type must implement <seealso cref="INotifyPropertyChanged"/>.</typeparam>
        /// <param name="source">The object to observe property changes on.</param>
        /// <returns>Returns an observable sequence with the source as its value. Values are produced each time the PropertyChanged event is raised.</returns>
        public static IObservable<T> AnyPropertyChanges<T>(this T source)
            where T : class, INotifyPropertyChanged
        {
            if (source == null) throw new ArgumentNullException("source");

            return Observable.FromEventPattern
                <PropertyChangedEventHandler, PropertyChangedEventArgs>
                (
                    h => source.PropertyChanged += h,
                    h => source.PropertyChanged -= h
                )
                .Select(_ => source);
        }        
    }
    public static class PropertyExtensions
    {
        /// <summary>
        /// Gets property information for the specified <paramref name="property"/> expression.
        /// </summary>
        /// <typeparam name="TSource">Type of the parameter in the <paramref name="property"/> expression.</typeparam>
        /// <typeparam name="TValue">Type of the property's value.</typeparam>
        /// <param name="property">The expression from which to retrieve the property information.</param>
        /// <returns>Property information for the specified expression.</returns>
        /// <exception cref="ArgumentException">The expression is not understood.</exception>
        public static PropertyInfo GetPropertyInfo<TSource, TValue>(this Expression<Func<TSource, TValue>> property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            var body = property.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Expression is not a property", "property");

            var propertyInfo = body.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Expression is not a property", "property");

            return propertyInfo;
        }
    }
}