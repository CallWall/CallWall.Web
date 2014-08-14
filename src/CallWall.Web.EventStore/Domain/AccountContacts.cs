using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore.Domain
{
    //TODO: Consider loading previous evens to see if there has been a incomplete RefreshCommand i.e. THe server failed during a Refresh. -LC
    public interface IAccountContacts
    {
        void RequestRefresh();
    }

    public sealed class AccountContacts : IAccountContacts
    {
        public void RequestRefresh()
        {
            throw new NotImplementedException();
        }
    }

    //internal class AccountContacts : IAccountContacts
    //{
    //    #region Private fields

    //    private readonly IEventStore _eventStore;
    //    private readonly IAccountContactProvider _accountContactProvider;
    //    private readonly string _providerName;
    //    private readonly string _accountId;
    //    private readonly SingleAssignmentDisposable _accountEventsSubscription = new SingleAssignmentDisposable();

    //    #endregion

    //    public AccountContacts(IEventStore eventStore, IAccountContactProvider accountContactProvider, string providerName, string accountId)
    //    {
    //        _eventStore = eventStore;
    //        _accountContactProvider = accountContactProvider;
    //        _providerName = providerName;
    //        _accountId = accountId;
    //    }

    //    public void Start()
    //    {
    //        //I think this will need to take an IAccountContactsProvider
    //        // It will need to be able to execute the contactProvider's getContactsFeed() method passing in relevant state (OAuth tokens, lastUpdate Timestamp/date/eventId)
    
    //        throw new NotImplementedException();

    //        //_accountEventsSubscription.Disposable = _eventStore.GetNewEvents(Account.StreamName(_providerName, _accountId))
    //        //                                        //.Where(evt=>evt.EventType==Account.EventType.)
    //        //                                        .Subscribe(x => { throw new NotImplementedException(); });
    //    }

    //    public void RequestRefresh()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Dispose()
    //    {
    //        //_eventStore.Dispose();
    //    }

        
    //    public static string StreamName(string providerName, string accountId)
    //    {
    //        return string.Format(@"AccountContacts-{0}-{1}", providerName, accountId);
    //    }
    //    public static class EventType
    //    {
    //        public static readonly string ContactsRefreshRequested = "ContactsRefreshRequested";
    //        public static readonly string ContactsRefreshCompleted = "ContactsRefreshCompleted";
    //        public static readonly string ContactSummaryUpdated = "ContactSummaryUpdated";
    //    }
    //}
}