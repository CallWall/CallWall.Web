using System;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Contacts;
using EventStore.ClientAPI;

namespace CallWall.Web.EventStore.Accounts
{
    public sealed class AccountContactRefresher : IAccountContactRefresher
    {
        private readonly IEventStoreClient _eventStoreClient;

        public AccountContactRefresher(IEventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
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

            await _eventStoreClient.SaveEvent(ContactStreamNames.AccountRefreshRequests(), ExpectedVersion.Any, Guid.NewGuid(), ContactEventType.AccountContactRefreshRequest, evt);
        }
    }
}