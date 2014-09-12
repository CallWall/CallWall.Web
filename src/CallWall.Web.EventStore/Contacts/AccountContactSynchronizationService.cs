using System;
using System.Collections.Generic;
using System.Diagnostics;
using CallWall.Web.EventStore.Users;
using EventStore.ClientAPI;

namespace CallWall.Web.EventStore.Contacts
{
    //public sealed class AccountContactSynchronizationService : AllEventListenerBase
    //{
    //    private readonly IAccountContactsFactory _accountContactsFactory;
    //    private readonly Dictionary<string, AccountContacts> _accounts = new Dictionary<string, AccountContacts>();
    //    public AccountContactSynchronizationService(IEventStoreConnectionFactory connectionFactory, IAccountContactsFactory accountContactsFactory)
    //        : base(connectionFactory)
    //    {
    //        _accountContactsFactory = accountContactsFactory;
    //    }

    //    protected override void OnEventReceived(ResolvedEvent resolvedEvent)
    //    {
    //        switch (resolvedEvent.OriginalEvent.EventType)
    //        {
    //            case UserEventType.UserCreated:
    //                AddAccount(resolvedEvent.OriginalEvent);
    //                break;
    //                //case AccountEventType.AccountRegististered:
    //                //    AddAccount();
    //                //    break;
    //                //case AccountEventType.AccountDeregistered:
    //                //case AccountEventType.AccountRevoked:
    //                //    //Strip out contacts from old account. May require a re-parse of contacts from existing accounts.
    //                //    break;
    //            case ContactEventType.AccountContactRefreshRequest:
    //                ProcessRefreshRequest(resolvedEvent.OriginalEvent);
    //                break;
    //                //case AccountEventType.SessionRefresh
    //                //  UpdateAccountSession(resolvedEvent.OriginalEvent);
    //                //  break;
    //        }
    //    }

    //    private void AddAccount(RecordedEvent userCreatedEvent)
    //    {
    //        var payload = userCreatedEvent.Deserialize<UserCreatedEvent>();
    //        var key = GetKey(payload.Account.Provider, payload.Account.AccountId);

    //        //TODO: What do I do if there are duplicates or it is missing? -LC
    //        var accountContacts = _accountContactsFactory.Create(payload.Account);
    //        _accounts[key] = accountContacts;
    //    }

    //    private void ProcessRefreshRequest(RecordedEvent recordedEvent)
    //    {
    //        var payload = recordedEvent.Deserialize<RefreshContactsCommand>();
    //        var key = GetKey(payload.Provider, payload.AccountId);
    //        _accounts[key].RequestRefresh();
    //    }

    //    private static string GetKey(string provider, string accountId)
    //    {
    //        return string.Format("{0}-{1}", provider, accountId);
    //    }
    //}

    public sealed class AccountContactSynchronizationService : DomainEventBase
    {
        private readonly IAccountContactsFactory _accountContactsFactory;
        private readonly Dictionary<string, AccountContacts> _accounts = new Dictionary<string, AccountContacts>();
        public AccountContactSynchronizationService(IEventStoreClient eventStoreClient, IAccountContactsFactory accountContactsFactory)
            : base(eventStoreClient, ContactStreamNames.AccountRefreshRequests())
        {
            _accountContactsFactory = accountContactsFactory;
        }

        protected override void OnEventReceived(ResolvedEvent resolvedEvent)
        {
            switch (resolvedEvent.OriginalEvent.EventType)
            {
                case UserEventType.UserCreated:
                    AddAccount(resolvedEvent.OriginalEvent);
                    break;
                //case AccountEventType.AccountRegististered:
                //    AddAccount();
                //    break;
                //case AccountEventType.AccountDeregistered:
                //case AccountEventType.AccountRevoked:
                //    //Strip out contacts from old account. May require a re-parse of contacts from existing accounts.
                //    break;
                case ContactEventType.AccountContactRefreshRequest:
                    ProcessRefreshRequest(resolvedEvent.OriginalEvent);
                    break;
                //case AccountEventType.SessionRefresh
                //  UpdateAccountSession(resolvedEvent.OriginalEvent);
                //  break;
            }
        }

        protected override void OnStreamError(Exception error)
        {
            throw new NotImplementedException();
        }

        private void AddAccount(RecordedEvent userCreatedEvent)
        {
            var payload = userCreatedEvent.Deserialize<UserCreatedEvent>();
            var key = GetKey(payload.Account.Provider, payload.Account.AccountId);

            //TODO: What do I do if there are duplicates or it is missing? -LC
            var accountContacts = _accountContactsFactory.Create(payload.Account);
            _accounts[key] = accountContacts;
        }

        private void ProcessRefreshRequest(RecordedEvent recordedEvent)
        {
            Trace.WriteLine("Processing Refresh request");
            var payload = recordedEvent.Deserialize<RefreshContactsCommand>();
            var accountContacts = GetOrCreateAccountContacts(payload);
            accountContacts.RequestRefresh(payload.UserId);
        }

        private AccountContacts GetOrCreateAccountContacts(IAccountData payload)
        {
            var key = GetKey(payload.Provider, payload.AccountId);
            AccountContacts accountContacts;
            if (!_accounts.TryGetValue(key, out accountContacts))
            {
                accountContacts = _accountContactsFactory.Create(payload);
                _accounts[key] = accountContacts;
            }
            return accountContacts;
        }

        private static string GetKey(string provider, string accountId)
        {
            return string.Format("{0}-{1}", provider, accountId);
        }
    }
}