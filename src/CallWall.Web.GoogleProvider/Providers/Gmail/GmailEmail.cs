using System;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Communication;

namespace CallWall.Web.GoogleProvider.Providers.Gmail
{
    public sealed class GmailEmail : IMessage
    {
        private readonly DateTimeOffset _timestamp;
        private readonly MessageDirection _direction;
        private readonly string _subject;
        private readonly string _deepLink;

        public GmailEmail(DateTimeOffset timestamp, MessageDirection direction, string subject, string deeplink)
        {
            _timestamp = timestamp;
            _direction = direction;
            _subject = subject;
            _deepLink = deeplink;
        }

        public DateTimeOffset Timestamp { get { return _timestamp; } }

        public MessageDirection Direction { get { return _direction; } }

        public string Subject { get { return _subject; } }

        public string DeepLink { get { return _deepLink; }  }


        IProviderDescription IMessage.Provider { get { return GmailProviderDescription.Instance; } }

        MessageType IMessage.MessageType { get { return MessageType.Email; } }

        string IMessage.Content { get { return null; } }

        public override string ToString()
        {
            return string.Format("Gmail {{{0:o} {1} Subject:'{2}'}}", Timestamp, Direction, Subject.TrimTo(50));
        }
    }
}