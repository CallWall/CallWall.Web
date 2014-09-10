using System;
using System.Collections.Generic;
using System.Diagnostics;
using CallWall.Web.EventStore.Users;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;

namespace CallWall.Web.EventStore.Contacts
{
    public sealed class UserContactSynchronizationService : AllEventListenerBase, IContactSynchronizationService
    {
        private readonly EventStore _eventStore;
        private readonly Dictionary<string, Guid> _accountIdToUserId = new Dictionary<string, Guid>();
        private readonly Dictionary<Guid, UserContacts> _users = new Dictionary<Guid, UserContacts>();

        public UserContactSynchronizationService(IEventStoreConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
            _eventStore = new EventStore(connectionFactory);
        }


        protected override void OnEventReceived(ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.OriginalEvent.EventType)
            {
                case UserEventType.UserCreated:
                    AddUser(resolvedEvent.OriginalEvent);
                    break;
                //case AccountEventType.AccountRegististered:
                //    AddAccount();
                //    break;
                //case AccountEventType.AccountDeregistered:
                //case AccountEventType.AccountRevoked:
                //    //Strip out contacts from old account. May require a re-parse of contacts from existing accounts.
                //    break;
                case ContactEventType.AccountContactUpdate:
                    UpdateContact(resolvedEvent.OriginalEvent);
                    break;
            }
        }

        private void AddUser(RecordedEvent userCreatedEvent)
        {
            var payload = userCreatedEvent.Deserialize<UserCreatedEvent>();
            Trace.WriteLine("Adding account '" + payload.Account.AccountId + "' to user '" + payload.Id + "'");
            _accountIdToUserId[payload.Account.AccountId] = payload.Id;
        }

        private void UpdateContact(RecordedEvent originalEvent)
        {
            var payload = originalEvent.Deserialize<AccountContactBatchUpdateRecord>();
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
            var payload = userContacts.GetChangesSnapshot().ToJson();
            _eventStore.SaveEvent(streamName, userContacts.Version, Guid.NewGuid(),
                ContactEventType.UserAggregateContactUpdate, payload)
                .Wait();
        }

        private UserContacts GetUserContacts(AccountContactBatchUpdateRecord payload)
        {
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

    //Listening to all events seems to do never yield a value? -LC 
    //  --> TRYING TURNING ON PROJECTSION FROM THE CMDLINE -LC  
    //public sealed class UserContactSynchronizationService : DomainEventBase, IContactSynchronizationService
    //{
    //    private readonly EventStore _eventStore;
    //    private readonly Dictionary<string, Guid> _accountIdToUserId = new Dictionary<string, Guid>();
    //    private readonly Dictionary<Guid, UserContacts> _users = new Dictionary<Guid, UserContacts>();

    //    public UserContactSynchronizationService(IEventStoreConnectionFactory connectionFactory)
    //        : base(connectionFactory, "AccountRegistrationStream?")
    //    {
    //        _eventStore = new EventStore(connectionFactory);
    //    }

    //    protected override void OnEventReceived(ResolvedEvent resolvedEvent)
    //    {
    //        switch (resolvedEvent.OriginalEvent.EventType)
    //        {
    //            case UserEventType.UserCreated:
    //                AddUser(resolvedEvent.OriginalEvent);
    //                break;
    //            //case AccountEventType.AccountRegististered:
    //            //    AddAccount();
    //            //    break;
    //            //case AccountEventType.AccountDeregistered:
    //            //case AccountEventType.AccountRevoked:
    //            //    //Strip out contacts from old account. May require a re-parse of contacts from existing accounts.
    //            //    break;
    //            case ContactEventType.AccountContactUpdate:
    //                UpdateContact(resolvedEvent.OriginalEvent);
    //                break;
    //        }
    //    }

    //    protected override void OnStreamError(Exception error)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private void AddUser(RecordedEvent userCreatedEvent)
    //    {
    //        var payload = userCreatedEvent.Deserialize<UserCreatedEvent>();
    //        _accountIdToUserId[payload.Account.AccountId] = payload.Id;
    //    }

    //    private void UpdateContact(RecordedEvent originalEvent)
    //    {
    //        var payload = originalEvent.Deserialize<AccountContactBatchUpdateRecord>();
    //        var userContacts = GetUserContacts(payload);

    //        using (userContacts.TrackChanges())
    //        {
    //            foreach (var contact in payload.Contacts)
    //            {
    //                userContacts.Add(contact);
    //            }

    //            SaveUpdates(userContacts);

    //            userContacts.CommitChanges();
    //        }

    //        throw new NotImplementedException();
    //    }

    //    private void SaveUpdates(UserContacts userContacts)
    //    {
    //        var streamName = ContactStreamNames.UserContacts(userContacts.UserId);
    //        var payload = userContacts.GetChangesSnapshot().ToJson();
    //        _eventStore.SaveEvent(streamName, userContacts.Version, Guid.NewGuid(),
    //            ContactEventType.UserAggregateContactUpdate, payload)
    //            .Wait();
    //    }

    //    private UserContacts GetUserContacts(AccountContactBatchUpdateRecord payload)
    //    {
    //        var userId = _accountIdToUserId[payload.AccountId];

    //        //I believe that this is serialized and non reentrant, thus thread safe.
    //        UserContacts userContacts;
    //        if (!_users.TryGetValue(userId, out userContacts))
    //        {
    //            userContacts = new UserContacts(userId);
    //            _users.Add(userId, userContacts);
    //        }
    //        return userContacts;
    //    }
    //}
}