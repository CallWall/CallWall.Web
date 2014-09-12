using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CallWall.Web.EventStore.Events;
using CallWall.Web.Providers;
using EventStore.ClientAPI;

namespace CallWall.Web.EventStore
{
    class ContactSummaryRepository : IContactSummaryRepository
    {
        private readonly IEventStoreClient _eventStoreClient;

        public ContactSummaryRepository(IEventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        public IObservable<IContactSummaryUpdate> GetContactUpdates(int userId, int fromEventId)
        {
            return Observable.Create<IContactSummaryUpdate>(o =>
            {
                var refreshCommand = new RefreshContactSummaryCommand(userId, fromEventId);
                _eventStoreClient.SaveEvent(StreamNames.ContactSummaryRefreshRequest, ExpectedVersion.Any, Guid.NewGuid(), refreshCommand);

                return _eventStoreClient.GetEvents<ContactSummaryUpdate>(StreamNames.ContactSummaryUpdates(userId))
                                  .Subscribe(o);
            });
        }
    }

    //TODO: Create an IModule implementation (for when being hosted by Web)

    class ContactSummaryRefreshProcessor : IDisposable
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();
        private bool _isRunning;

        public ContactSummaryRefreshProcessor(IEventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        public void Run()
        {
            if (_isRunning)
                return;
            _isRunning = true;

            //TODO: Add throttling by User/EventId i.e. 
            //  1) If user hit refresh 3 times in 10 seconds, ignore the later 2?
            //  2) If previous refresh is still processing, do not reissue?
            var query = from refreshCommand in _eventStoreClient.GetNewEvents<RefreshContactSummaryCommand>(StreamNames.ContactSummaryRefreshRequest)
                        from sessions in GetSessionsByUserId(refreshCommand.UserId)
                        select sessions;

            _subscription.Disposable = query.ObserveOn(Scheduler.Default)
                                            .Subscribe(EnqueueContactSummaryRefresh, ex => { }, () => { });
        }

        private void EnqueueContactSummaryRefresh(ISession session)
        {
            //TODO: Need to track a provider specific way of time stamping this stuff.
            _eventStoreClient.SaveEvent(StreamNames.ContactSummaryProviderRefreshRequest, ExpectedVersion.Any, Guid.NewGuid(), session);
        }

        //TODO:
        public IObservable<ISession> GetSessionsByUserId(int userId){return null;}



        public void Dispose()
        {
            _subscription.Dispose();
        }
    }

    class ContactSummaryProviderRefreshProcessor : IDisposable
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly IEnumerable<IAccountContactProvider> _providers;
        private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();
        private bool _isRunning;

        public ContactSummaryProviderRefreshProcessor(IEventStoreClient eventStoreClient, IEnumerable<IAccountContactProvider> providers)
        {
            _eventStoreClient = eventStoreClient;
            _providers = providers;
        }

        public void Run()
        {
            throw new NotImplementedException(); //Throwing until I get the new arch done. -LC

            if (_isRunning)
                return;
            _isRunning = true;

            //HACK: Get this from somewhere
            var userId = 27;
            //HACK: Get this from somewhere
            var lastUpdate = new DateTime();


            //var query =
            //    from session in _eventStore.GetNewEvents<ISession>(StreamNames.ContactSummaryProviderRefreshRequest)
            //    from provider in _providers
            //    from feed in provider.GetContactsFeed(session, lastUpdate)
            //    from contact in feed.Values
            //    select new
            //    {
            //        ContactSummary = contact,
            //        Session = session
            //    };

            //_subscription.Disposable = query.ObserveOn(Scheduler.Default)
            //                                .Subscribe(i => OnContactSummaryRecieved(userId, i.Session.Provider, i.ContactSummary));


        }

        private void OnContactSummaryRecieved(int userId, string provider, IAccountContactSummary contactSummary)
        {
            var streamName = StreamNames.ContactSummaryRecieved(userId, provider);
            _eventStoreClient.SaveEvent(streamName, ExpectedVersion.Any, Guid.NewGuid(), contactSummary);
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}