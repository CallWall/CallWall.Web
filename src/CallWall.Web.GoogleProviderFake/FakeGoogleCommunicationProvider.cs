using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CallWall.Web.Domain;
using CallWall.Web.Providers;
using CallWall.Web.Contracts.Communication;

namespace CallWall.Web.GoogleProviderFake
{
    //TODO: MOve all referenced images to Fake's content path. Have it copied on build to correct path as per other modules. -LC
    // This means passing the uri instead of a handle like "hangouts" as the client wont know what that means esp when we add 100s' more providers. -LC
    public class FakeGoogleCommunicationProvider : ICommunicationProvider
    {
        public IObservable<IMessage> GetMessages(User user, string[] contactKeys)
        {
            return Observable.Interval(TimeSpan.FromSeconds(1))
                .Zip(GetMessages(), (_, msg) => msg);
        }

        private static IEnumerable<IMessage> GetMessages()
        {
            var n = DateTimeOffset.Now;
            yield return new Message(n.AddMinutes(-10), false, "On my wayXX", null, ProviderDescription.Hangouts);
            yield return new Message(n.AddMinutes(-13), true, "Dude, where are you?", null, ProviderDescription.Hangouts);
            yield return new Message(n.AddDays(-2), false, "Pricing a cross example", "Here is the sample we were talking about the other day. It should cover the basic case, the complex multi-leg option case and all the variations in-between. If you have any questions, then just email me back on my home account.", ProviderDescription.LinkedIn);
            yield return new Message(n.AddDays(-4), false, "I will bring the food for the Rugby", "From: James Alex To: You, Lee FAKE Camplell, Simon Real, Brian Baxter, Josh Taylor and Sally Hubbard", ProviderDescription.Gmail);
            yield return new Message(n.AddDays(-4), false, "CallWall are recruiting engineers now!", "Retweets : 7", ProviderDescription.Twitter);
            yield return new Message(n.AddDays(-5), true, "Rugby at my place on Saturday morning", "To: James Alex, Simon Real + 3 others", ProviderDescription.Gmail);
        }

        class Message : IMessage
        {
            public DateTimeOffset Timestamp { get; private set; }
            public MessageDirection Direction { get; private set; }
            public string Subject { get; private set; }
            public string Content { get; private set; }
            public Contracts.IProviderDescription Provider { get; private set; }
            public MessageType MessageType { get; private set; }
            public string DeepLink { get { return null; } }

            public Message(DateTimeOffset timestamp, bool isOutbound, string subject, string content, Contracts.IProviderDescription provider)
            {
                Timestamp = timestamp;
                Direction = isOutbound ? MessageDirection.Outbound : MessageDirection.Inbound;
                Subject = subject;
                Content = content;
                Provider = provider;
                MessageType = MessageType.Email;
            }
        }
    }

    public sealed class ProviderDescription : Contracts.IProviderDescription
    {
        public static readonly ProviderDescription Gmail = new ProviderDescription("Gmail", "/Content/Fakes/Images/Gmail_48x48.png");
        public static readonly ProviderDescription Hangouts = new ProviderDescription("Hangouts", "/Content/Fakes/Images/HangoutChat_48x48.png");
        public static readonly ProviderDescription LinkedIn = new ProviderDescription("LinkedIn", "/Content/Fakes/Images/LinkedInMail_48x48.png");
        public static readonly ProviderDescription Twitter = new ProviderDescription("Twitter", "/Content/Fakes/Images/Tweet_48x48.png");


        private ProviderDescription(string name, string imageUri)
        {
            Name = name;
            Image = new Uri(imageUri, UriKind.Relative);
        }

        public string Name { get; private set; }
        public Uri Image { get; private set; }
    }

}