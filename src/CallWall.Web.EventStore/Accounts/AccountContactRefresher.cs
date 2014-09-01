using System;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Contacts;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;

namespace CallWall.Web.EventStore.Accounts
{
    public sealed class AccountContactRefresher : IAccountContactRefresher
    {
        private readonly EventStore _eventStore;

        public AccountContactRefresher(IEventStoreConnectionFactory connectionFactory)
        {
            _eventStore = new EventStore(connectionFactory);
        }

        public async Task RequestRefresh(Guid userId, IAccount account, ContactRefreshTriggers triggeredBy)
        {
            var evt = new RefreshContactsCommand
            {
                RefreshTrigger = triggeredBy.ToString(),
                TimeStamp = DateTimeOffset.Now,
                UserId = userId,
                Provider = account.Provider,
                AccountId = account.AccountId,
                DisplayName = account.DisplayName,
                CurrentSession = new SessionRecord(account.CurrentSession)
            }.ToJson();

            await _eventStore.SaveEvent(ContactStreamNames.AccountRefreshRequests(), ExpectedVersion.Any, Guid.NewGuid(), ContactEventType.AccountContactRefreshRequest, evt);
        }
    }
}