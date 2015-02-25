using System;

namespace CallWall.Web.Contracts.Communication
{
    public interface IMessage
    {
        /// <summary>
        /// The date and time this message was transmitted.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// An indicator to specify if this message was sent or received.
        /// </summary>
        MessageDirection Direction { get; }

        /// <summary>
        /// The subject of the message. All message must carry a subject.
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// Optional content of the message. Short message formats (e.g. Tweets, SMS) will not have a content section.
        /// </summary>
        /// <remarks>
        /// It is likely that if any of the content is displayed it will be truncated.
        /// </remarks>
        string Content { get; }

        /// <summary>
        /// The related provider of the message.
        /// </summary>
        IProviderDescription Provider { get; }

        MessageType MessageType { get; }

        /// <summary>
        /// A means of linking from the CallWall app or website into the application or site directly to the message.
        /// </summary>
        string DeepLink { get; }
    }

    public enum MessageType
    {
        Unknown,
        Email,
        InstantMessage,
        Tweet,   //? Broadcast?
        Sms
    }
}