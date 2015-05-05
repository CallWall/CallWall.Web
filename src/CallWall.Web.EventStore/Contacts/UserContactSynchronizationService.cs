using System;
using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Users;
using EventStore.ClientAPI;

namespace CallWall.Web.EventStore.Contacts
{
    public sealed class UserContactSynchronizationService : AllEventListenerBase
    {
        private readonly Dictionary<string, Guid> _accountIdToUserId = new Dictionary<string, Guid>();
        private readonly Dictionary<Guid, UserContacts> _users = new Dictionary<Guid, UserContacts>();

        public UserContactSynchronizationService(IEventStoreClient eventStoreClient, ILoggerFactory loggerFactory)
            : base(eventStoreClient, loggerFactory)
        {
            Logger.Info("UserContactSynchronizationService ctor");
        }
        
        protected override void OnEventReceived(ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.OriginalEvent.EventType)
            {
                case UserEventType.UserCreated:
                    AddUser(resolvedEvent.OriginalEvent);
                    break;
                case UserEventType.AccountRegistered:
                    AddAccount(resolvedEvent.OriginalEvent);
                    break;
                //case AccountEventType.AccountDeregistered:
                //case AccountEventType.AccountRevoked:
                //    //Strip out contacts from old account. May require a re-parse of contacts from existing accounts.
                //    break;
                case ContactEventType.AccountContactUpdate:
                    UpdateContact(resolvedEvent.OriginalEvent);
                    break;
            }
        }

        private void AddAccount(RecordedEvent accountRegisteredEvent)
        {
            var payload = accountRegisteredEvent.Deserialize<UserRegisteredAccountEvent>();
            LinkUserIdToAccountId(payload.UserId, payload.Account.AccountId);
        }

        private void AddUser(RecordedEvent userCreatedEvent)
        {
            var payload = userCreatedEvent.Deserialize<UserCreatedEvent>();
            LinkUserIdToAccountId(payload.Id, payload.Account.AccountId);
        }

       
        private void UpdateContact(RecordedEvent originalEvent)
        {
            var payload = originalEvent.Deserialize<AccountContactBatchUpdateRecord>();
            Logger.Trace("Updating {0} contact(s) for Account '{1}' on user '{2}'", payload.Contacts.Length, payload.AccountId, payload.UserId);
            var userContacts = GetUserContacts(payload);

            using (userContacts.TrackChanges())
            {
                foreach (var contact in payload.Contacts)
                {
                    userContacts.Add(contact);
                }

                SaveUpdates(userContacts);

                userContacts.CommitChanges();
            }
        }

        private void SaveUpdates(UserContacts userContacts)
        {
            var streamName = ContactStreamNames.UserContacts(userContacts.UserId);
            var payload = userContacts.GetChangesSnapshot().Select(change=>change.ToJson()).ToArray();

            SaveBatch(streamName, userContacts.Version, ContactEventType.UserAggregateContactUpdate, payload)
                .Wait();
        }

        private void LinkUserIdToAccountId(Guid userId, string accountId)
        {
            Logger.Trace("Adding account '{0}' to user '{1}'", accountId, userId);
            _accountIdToUserId[accountId] = userId;
        }

        private UserContacts GetUserContacts(AccountContactBatchUpdateRecord payload)
        {
            if (!_accountIdToUserId.ContainsKey(payload.AccountId))
            {
                Logger.Error("AccountId '{0}' not found in cache.", payload.AccountId);
                Logger.Info("Existing keys : {0}", string.Join(",", _accountIdToUserId.Keys));
            }

            var userId = _accountIdToUserId[payload.AccountId];

            //I believe that this is serialized and non reentrant, thus thread safe.
            UserContacts userContacts;
            if (!_users.TryGetValue(userId, out userContacts))
            {
                userContacts = new UserContacts(userId);
                _users.Add(userId, userContacts);
            }
            return userContacts;
        }
    }    
}