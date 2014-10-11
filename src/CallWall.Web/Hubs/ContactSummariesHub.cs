using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.Providers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    //TODO: As each request to a Hub will create a new Hub, move to a pattern where a Hub has a single method 'Subscribe(?)'
    //  Each client will then be simply an Observer eg. only have OnNext, OnError, OnCompleted methods.

    [HubName("contactSummaries")]
    public class ContactSummariesHub : Hub
    {
        private readonly IContactSummaryRepository _contactSummaryRepository;
        private readonly ILoginProvider _loginProvider;
        private readonly ILogger _logger; 
        private readonly SerialDisposable _contactsSummarySubsription = new SerialDisposable();
        private readonly SerialDisposable _headVersionSubsription = new SerialDisposable();

        public ContactSummariesHub(IContactSummaryRepository contactSummaryRepository, ILoginProvider loginProvider, ILoggerFactory loggerFactory)
        {
            _contactSummaryRepository = contactSummaryRepository;
            _loginProvider = loginProvider;
            _logger = loggerFactory.CreateLogger(GetType());
            _logger.Info("ContactSummariesHub.ctor()");
        }

        //public void RequestContactSummaryStream(ClientLastUpdated[] lastUpdatedDetails)
        //{
        //    Debug.Print("ContactSummariesHub.RequestContactSummaryStream(...)");
        //    var sessions = _sessionProvider.GetSessions(Context.User);
        //    var subscription = _contactsProviders
        //                        .ToObservable()
        //                        .SelectMany(c => c.GetContactsFeed(sessions, lastUpdatedDetails))
        //                        .Do(feed => Clients.Caller.ReceivedExpectedCount(feed.TotalResults))
        //                        .SelectMany(feed => feed.Values)
        //                        .Log(_logger, "GetContactsFeed")
        //                        .Subscribe(contact => Clients.Caller.ReceiveContactSummary(contact),
        //                                   ex => Clients.Caller.ReceiveError("Error receiving contacts"),
        //                                   () => Clients.Caller.ReceiveComplete(sessions.Select(s => new ClientLastUpdated{
        //                                        Provider = s.Provider,
        //                                        LastUpdated = DateTime.UtcNow, 
        //                                        Revision = lastUpdatedDetails.Where(l=>l.Provider == s.Provider)
        //                                                                     .Select(l=>l.Revision)
        //                                                                     .FirstOrDefault() 
        //                                   })));

        //    _contactsSummarySubsription.Disposable = subscription;
        //}

        public async Task RequestContactSummaryStream(int fromEventId)
        {
            try
            {
                _logger.Debug("ContactSummariesHub.RequestContactSummaryStream({0})", fromEventId);            
                var user = await _loginProvider.GetUser(Context.User.UserId());
                _logger.Trace("Getting contacts for user : {0}", user.Id);
                var subscription = _contactSummaryRepository.GetContactUpdates(user, fromEventId)
                    .Log(_logger, "RequestContactSummaryStream")
                    .Subscribe(
                        contactUpdate => Clients.Caller.ReceiveContactSummaryUpdate(contactUpdate),
                        ex => Clients.Caller.ReceiveError("Error receiving contacts"));

                _contactsSummarySubsription.Disposable = subscription;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Requesting ContactSummaryStream in Hub.");
                Clients.Caller.ReceiveError("Error receiving contacts");
            }
        }
        
        public async Task RequestHeadVersionStream()
        {
            var user = await _loginProvider.GetUser(Context.User.UserId());
            var subscription = _contactSummaryRepository.ObserveContactUpdatesHeadVersion(user)
                .Buffer(TimeSpan.FromSeconds(0.5))
                .Where(buffer=>buffer.Count > 0)
                .Select(buffer=>buffer.Last())
                .Subscribe(
                serverVersion=>Clients.Caller.ReceiveContactSummaryServerHeadVersion(serverVersion),
                        ex => _logger.Error(ex, "RequestHeadVersionStream errored."));                

            _headVersionSubsription.Disposable = subscription;
        }

        public override Task OnDisconnected()
        {
            _logger.Debug("ContactSummariesHub.OnDisconnected()");
            _contactsSummarySubsription.Dispose();
            return base.OnDisconnected();
        }
    }
}
