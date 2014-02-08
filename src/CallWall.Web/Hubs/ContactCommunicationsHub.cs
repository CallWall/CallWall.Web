using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contactCommunications")]
    public class ContactCommunicationsHub : Hub
    {
        private readonly ILogger _logger;
        private readonly SerialDisposable _contactComunicationSubscription = new SerialDisposable();

        public ContactCommunicationsHub(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void Subscribe(string[] contactKeys)
        {
            try
            {
                _logger.Debug("RequestContactProfile({0})", string.Join(",", contactKeys));
                var subscription = GetMessages()
                .ToObservable(Scheduler.ThreadPool)
                .Subscribe(
                    message => Clients.Caller.OnNext(message),
                    ex =>
                    {
                        _logger.Error(ex, "Error in getting Messages");
                        Clients.Caller.OnError("Unable to get communications");
                    },
                    () => Clients.Caller.OnCompleted());
                _contactComunicationSubscription.Disposable = subscription;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RequestContactCommunications failure");
                Clients.Caller.OnError("Unable to get communications");
            }
        }

        private static IEnumerable<Message> GetMessages()
        {
            var n = DateTime.Now;
            yield return new Message(n.AddMinutes(-10), false, "On my way", null, "hangouts"); Thread.Sleep(TimeSpan.FromSeconds(2));
            yield return new Message(n.AddMinutes(-13), true, "Dude, where are you?", null, "hangouts"); Thread.Sleep(TimeSpan.FromSeconds(2));
            yield return new Message(n.AddDays(-2), false, "Pricing a cross example","Here is the sample we were talking about the other day. It should cover the basic case, the complex multi-leg option case and all the variations in-between. If you have any questions, then just email me back on my home account.","linkedin"); Thread.Sleep(TimeSpan.FromSeconds(2));
            yield return new Message(n.AddDays(-4), false, "I will bring the food for the Rugby","From: James Alex To: You, Lee FAKE Camplell, Simon Real, Brian Baxter, Josh Taylor and Sally Hubbard","gmail"); Thread.Sleep(TimeSpan.FromSeconds(2));
            yield return new Message(n.AddDays(-4), false, "CallWall are recruiting engineers now!", "Retweets : 7","twitter"); Thread.Sleep(TimeSpan.FromSeconds(2));
            yield return new Message(n.AddDays(-5), true, "Rugby at my place on Saturday morning","To: James Alex, Simon Real + 3 others", "gmail");
        }
    }

    public class Message
    {
        public DateTime Timestamp { get; set; }
        public bool IsOutbound { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Provider { get; set; }

        public Message() { }
        public Message(DateTime timestamp, bool isOutbound, string subject, string content, string provider)
        {
            Timestamp = timestamp;
            IsOutbound = isOutbound;
            Subject = subject;
            Content = content;
            Provider = provider;
        }
    }
}