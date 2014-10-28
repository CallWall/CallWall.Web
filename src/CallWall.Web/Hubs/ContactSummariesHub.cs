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
        
        public async Task RequestContactSummaryStream(int fromEventId)
        {
            try
            {
                _logger.Debug("ContactSummariesHub.RequestContactSummaryStream({0})", fromEventId);            
                var user = await _loginProvider.GetUser(Context.User.UserId());
                _logger.Trace("Getting contacts for user : {0}", user.Id);
                var subscription = _contactSummaryRepository.GetContactUpdates(user, fromEventId)
                    .Select(cau=>new ContactAggregateUpdateSummary(cau))
                    .Where(summary=>summary.IsRelevant)
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
    public class ContactAggregateUpdateSummary
    {
        public ContactAggregateUpdateSummary(ContactAggregateUpdate source)
        {
            Id = source.Id;
            Version = source.Version;
            IsDeleted = source.IsDeleted;
            NewTitle = source.NewTitle;
            AddedAvatars = source.AddedAvatars;
            RemovedAvatars = source.RemovedAvatars;
        }

        public int Id { get; set; }
        public int Version { get; set; }
        public bool IsDeleted { get; set; }
        public string NewTitle { get; set; }
        public string[] AddedAvatars { get; set; }
        public string[] RemovedAvatars { get; set; }

        public bool IsRelevant
        {
            get
            {
                return IsDeleted
                    || !string.IsNullOrWhiteSpace(NewTitle)
                    || AddedAvatars != null
                    || RemovedAvatars != null;
            }
        }


        public override string ToString()
        {
            if (IsDeleted)
            {
                return string.Format("ContactAggregateUpdate{{ Id:{0}, Version:{1}, IsDeleted:true}}", Id, Version);
            }
            return string.Format("ContactAggregateUpdate{{ Id:{0}, Version:{1}, NewTitle:{2}}}", Id, Version, NewTitle);
        }
    }
}
