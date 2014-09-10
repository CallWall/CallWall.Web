using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CallWall.Web.EventStore.Accounts;
using CallWall.Web.Providers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.Common.Utils;

namespace CallWall.Web.EventStore.Contacts
{
    public sealed class AccountContacts
    {
        private readonly EventStore _eventStore;
        private readonly IAccountContactProvider _contactProvider;
        private readonly string _accountId;
        private readonly string _provider;
        private readonly SerialDisposable _currentFeedRequest = new SerialDisposable();
        private DateTime _lastRefresh;

        //TODO: Potentially subclass DEB -LC
        private int _writeVersion = ExpectedVersion.NoStream;

        private readonly Dictionary<string, IAccountContactSummary> _contactSummariesSnapShot = new Dictionary<string, IAccountContactSummary>();
        private readonly Dictionary<string, IAccountContactSummary> _contactSummaries = new Dictionary<string, IAccountContactSummary>();
        private readonly List<IAccountContactSummary> _changes = new List<IAccountContactSummary>();


        public AccountContacts(
            IEventStoreConnectionFactory connectionFactory,
            IAccountContactProvider contactProvider,
            IAccountData accountData)
        {
            if (contactProvider == null) throw new ArgumentNullException("contactProvider");
            if (accountData == null) throw new ArgumentNullException("provider");
            if (contactProvider.Provider != accountData.Provider) throw new InvalidOperationException("Provider must match the provider for the accountContactProvider");

            _eventStore = new EventStore(connectionFactory);
            _contactProvider = contactProvider;
            _accountId = accountData.AccountId;
            _provider = accountData.Provider;
            CurrentSession = accountData.CurrentSession;
        }

        private ISession CurrentSession { get; set; }

        public void RequestRefresh(Guid userId)
        {
            //TODO: Check if it is valid to execute a refresh (isRunning, lastCompletedTime) -LC
            //TODO: Only run if not in replay mode? -LC

            TakeSnapshot();
            var account = GenerateAccount();

            var query = from feed in _contactProvider.GetContactsFeed(account, _lastRefresh)
                        from row in feed.Values
                        select row;

            _currentFeedRequest.Disposable = Observable.Create<Unit>(
                async (o, ct) =>
                {
                    var contacts = await query.ToList();
                    foreach (var contact in contacts)
                    {
                        UpdateContact(contact);
                    }
                    await UpdateComplete(userId);
                }).Subscribe(_ => { }, UpdateFailed);

        }

        private void UpdateContact(IAccountContactSummary updatedContact)
        {
            Trace.WriteLine("Updating contact");
            IAccountContactSummary existingContact;
            if (_contactSummaries.TryGetValue(updatedContact.ProviderId, out existingContact))
            {
                if (ContactComparer.Instance.Equals(existingContact, updatedContact))
                    return;
            }
            //  mark as modified
            _contactSummaries[updatedContact.ProviderId] = updatedContact;
            _changes.Add(updatedContact);
        }

        private void UpdateFailed(Exception error)
        {
            Trace.WriteLine("UpdateFailed - " + error);
            //  Rollback any updates
            RollbackToSnapshot();

            //  Push the failure to the ES. (retry?)
            //throw new NotImplementedException();
        }

        private async Task UpdateComplete(Guid userId)
        {
            var payload = GetChangesBatch(userId);
            await CommitChanges(payload);
        }


        private IAccountData GenerateAccount()
        {
            return new AccountRecord
            {
                AccountId = _accountId,
                Provider = _provider,
                CurrentSession = new SessionRecord(CurrentSession)
            };
        }

        private void TakeSnapshot()
        {
            _contactSummariesSnapShot.Clear();
            foreach (var kvp in _contactSummaries)
            {
                _contactSummariesSnapShot.Add(kvp.Key, kvp.Value);
            }
        }

        private void RollbackToSnapshot()
        {
            _contactSummaries.Clear();
            foreach (var kvp in _contactSummariesSnapShot)
            {
                _contactSummaries.Add(kvp.Key, kvp.Value);
            }
        }

        private async Task CommitChanges(string payload)
        {
            Trace.WriteLine("Committing changes");
            //  push changes batch to ES

            //TODO: Do I make a single stream for all AccountContact updates? Or do i subscribe to all streams as they accounts are registered?-LC

            var streamName = ContactStreamNames.AccountContacts(_provider, _accountId);
            //await _eventStore.SaveEvent(streamName, ExpectedVersion.Any, new Guid(), ContactEventType.AccountContactUpdate, payload);
            await _eventStore.SaveEvent(streamName, _writeVersion, Guid.NewGuid(), ContactEventType.AccountContactUpdate, payload);

            _writeVersion++;

            Trace.WriteLine("Committed changes");
            //  mark all as unmodified. 
            _changes.Clear();
            _contactSummariesSnapShot.Clear();
        }

        private string GetChangesBatch(Guid userId)
        {
            var changes = _changes.Select(c => new AccountContactRecord
                {
                    AccountId = c.AccountId,
                    Provider = c.Provider,
                    ProviderId = c.ProviderId,
                    Title = c.Title,
                    PrimaryAvatar = c.PrimaryAvatar,
                    Tags = c.Tags.ToArray()
                })
                .ToArray();

            Trace.WriteLine("Changes.count = " + changes.Length);
            var batch = new AccountContactBatchUpdateRecord
            {
                UserId = userId,
                AccountId = _accountId,
                Provider = _provider,
                Contacts = changes
            };
            var payload = batch.ToJson();
            return payload;
        }
    }
}