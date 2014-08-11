using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography;
using CallWall.Web.EventStore.Events;
using CallWall.Web.Providers;
using Newtonsoft.Json;

namespace CallWall.Web.EventStore
{
    class ContactSummaryRepository : IContactSummaryRepository
    {
        private readonly IEventStore _eventStore;

        public ContactSummaryRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public IObservable<IContactSummaryUpdate> GetContactUpdates(int userId, int fromEventId)
        {
            return Observable.Create<IContactSummaryUpdate>(o =>
            {
                var refreshCommand = new RefreshContactSummaryCommand(userId, fromEventId);
                _eventStore.SaveEvent(StreamNames.ContactSummaryRefreshRequest, refreshCommand);

                return _eventStore.GetAllEvents(StreamNames.ContactSummaryUpdates(userId))
                                  .Select(JsonConvert.DeserializeObject<ContactSummaryUpdate>)
                                  .Subscribe(o);
            });
        }
    }

    //TODO: Create an IModule implementation (for when being hosted by Web)

    class ContactSummaryRefreshProcessor : IDisposable
    {
        private readonly IEventStore _eventStore;
        private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();
        private bool _isRunning;

        public ContactSummaryRefreshProcessor(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public void Run()
        {
            if (_isRunning)
                return;
            _isRunning = true;

            //TODO: Add throttling by User/EventId i.e. 
            //  1) If user hit refresh 3 times in 10 seconds, ignore the later 2?
            //  2) If previous refresh is still processing, do not reissue?
            var query = from refreshCommand in _eventStore.GetNewEvents<RefreshContactSummaryCommand>(StreamNames.ContactSummaryRefreshRequest)
                        from sessions in GetSessionsByUserId(refreshCommand.UserId)
                        select sessions;

            _subscription.Disposable = query.ObserveOn(Scheduler.Default)
                                            .Subscribe(EnqueueContactSummaryRefresh, ex => { }, () => { });
        }

        private void EnqueueContactSummaryRefresh(ISession session)
        {
            //TODO: Need to track a provider specific way of time stamping this stuff.
            _eventStore.SaveEvent(StreamNames.ContactSummaryProviderRefreshRequest, session);
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
        private readonly IEventStore _eventStore;
        private readonly IEnumerable<IAccountContactProvider> _providers;
        private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();
        private bool _isRunning;

        public ContactSummaryProviderRefreshProcessor(IEventStore eventStore, IEnumerable<IAccountContactProvider> providers)
        {
            _eventStore = eventStore;
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

        private void OnContactSummaryRecieved(int userId, string provider, IContactSummary contactSummary)
        {
            var streamName = StreamNames.ContactSummaryRecieved(userId, provider);
            _eventStore.SaveEvent(streamName, contactSummary);
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}