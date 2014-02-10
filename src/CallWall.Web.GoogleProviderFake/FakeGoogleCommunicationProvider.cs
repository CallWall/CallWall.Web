using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProviderFake
{
    public class FakeGoogleCommunicationProvider : ICommunicationProvider
    {
        public IObservable<IMessage> GetMessages(IEnumerable<ISession> session, string[] contactKeys)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                .Zip(GetMessages(), (_, msg) => msg);
        }

        private static IEnumerable<IMessage> GetMessages()
        {
            var n = DateTime.Now;
            yield return new Message(n.AddMinutes(-10), false, "On my wayXX", null, "hangouts");
            yield return new Message(n.AddMinutes(-13), true, "Dude, where are you?", null, "hangouts");
            yield return new Message(n.AddDays(-2), false, "Pricing a cross example", "Here is the sample we were talking about the other day. It should cover the basic case, the complex multi-leg option case and all the variations in-between. If you have any questions, then just email me back on my home account.", "linkedin");
            yield return new Message(n.AddDays(-4), false, "I will bring the food for the Rugby", "From: James Alex To: You, Lee FAKE Camplell, Simon Real, Brian Baxter, Josh Taylor and Sally Hubbard", "gmail");
            yield return new Message(n.AddDays(-4), false, "CallWall are recruiting engineers now!", "Retweets : 7", "twitter");
            yield return new Message(n.AddDays(-5), true, "Rugby at my place on Saturday morning", "To: James Alex, Simon Real + 3 others", "gmail");
        }

        class Message : IMessage
        {
            public DateTime Timestamp { get; private set; }
            public bool IsOutbound { get; private set; }
            public string Subject { get; private set; }
            public string Content { get; private set; }
            public string Provider { get; private set; }

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
}