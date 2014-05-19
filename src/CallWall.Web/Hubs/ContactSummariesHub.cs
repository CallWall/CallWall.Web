﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.EventStore;
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
        private readonly ISessionProvider _sessionProvider;
        private readonly ILogger _logger;   //TODO: Replace Debug.Print with logger + ConsoleListener
        private readonly SerialDisposable _contactsSummarySubsription = new SerialDisposable();

        public ContactSummariesHub(IContactSummaryRepository contactSummaryRepository, ISessionProvider sessionProvider, ILoggerFactory loggerFactory)
        {
            Debug.Print("ContactSummariesHub.ctor()");
            _contactSummaryRepository = contactSummaryRepository;
            _sessionProvider = sessionProvider;
            _logger = loggerFactory.CreateLogger(GetType());
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

        public void RequestContactSummaryStream(int fromEventId)
        {
            Debug.Print("ContactSummariesHub.RequestContactSummaryStream(...)");
            var userId = _sessionProvider.GetUserId(Context.User);
            var subscription = _contactSummaryRepository.GetContactUpdates(userId, fromEventId)
                                                        .Subscribe(
                                                               contactUpdate => Clients.Caller.ReceiveContactSummary(contactUpdate),
                                           ex => Clients.Caller.ReceiveError("Error receiving contacts"),
                                                               () => Clients.Caller.ReceiveComplete()); //This will never fire? -LC

            _contactsSummarySubsription.Disposable = subscription;
        }
        
        public override Task OnDisconnected()
        {
            Debug.Print("ContactSummariesHub.OnDisconnected()");
            _contactsSummarySubsription.Dispose();
            return base.OnDisconnected();
        }
    }
}
