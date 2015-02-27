using System;
using System.IO;
using System.Text;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.GoogleProvider.Providers.Gmail;
using CallWall.Web.GoogleProvider.Providers.Gmail.Imap;
using NSubstitute;
using NUnit.Framework;

namespace CallWall.Web.GoogleProvider.Tests.Providers.Gmail
{
    [TestFixture]
    [Timeout(1000)]
    public class FetchMessageOperationFixture
    {
        private const string Prefix = "CW01";
        private ulong _messageId;
        private FetchMessageOperation _sut;
        private string _accountEmailAddress;

        [SetUp]
        public void SetUp()
        {
            var rnd = new Random();
            _messageId = (ulong)rnd.Next();
            _accountEmailAddress = "Lee@gmail.com";
            _sut = Create(_messageId, _accountEmailAddress);
        }

        [Test]
        public void Should_send_Imap_fetch_command()
        {
            var expectedCommand = string.Format(
                "{0} FETCH {1} (BODY.PEEK[HEADER.FIELDS (FROM TO Message-ID Subject Date)] X-GM-THRID)\r\n",
                Prefix, _messageId);
            var sendStream = new MemoryStream();
            var receiveStreamReader = CreateReceiveStreamReader(Prefix, CreateStubMessage(), ToStandardFormat);
            var wasSent = _sut.Execute(Prefix, sendStream, receiveStreamReader);

            Assert.IsTrue(wasSent);
            var sentBytes = sendStream.ToArray();
            var sentText = Encoding.ASCII.GetString(sentBytes);
            Assert.AreEqual(expectedCommand, sentText);
        }

        [Test]
        public void Should_process_success_inbound_message()
        {
            var expectedEmail = CreateStubMessage();
            Assume.That(!expectedEmail.FromAddress.Contains(_accountEmailAddress));
            var expectedDeepLink = CreateDeepLink(_accountEmailAddress, expectedEmail.ThreadId);
            var sendStream = new MemoryStream();
            var receiveStreamReader = CreateReceiveStreamReader(Prefix, expectedEmail, ToStandardFormat);
            var wasSent = _sut.Execute(Prefix, sendStream, receiveStreamReader);

            Assert.IsTrue(wasSent);
            var actual = (IMessage)_sut.ExtractMessage();
            Assert.AreEqual(null, actual.Content);
            Assert.AreEqual(MessageDirection.Inbound, actual.Direction);
            Assert.AreEqual(MessageType.Email, actual.MessageType);
            Assert.AreEqual(GmailProviderDescription.Instance, actual.Provider);
            Assert.AreEqual(expectedEmail.Subject, actual.Subject);
            Assert.AreEqual(expectedEmail.TimeStamp, actual.Timestamp);
            Assert.AreEqual(expectedDeepLink, actual.DeepLink);
        }

        [Test]
        public void Should_process_success_outbound_message()
        {
            var expectedEmail = CreateStubMessage();
            expectedEmail.FromAddress = string.Format("Me <{0}>", _accountEmailAddress);
            Assume.That(expectedEmail.FromAddress.Contains(_accountEmailAddress));
            var expectedDeepLink = CreateDeepLink(_accountEmailAddress, expectedEmail.ThreadId);
            var sendStream = new MemoryStream();
            var receiveStreamReader = CreateReceiveStreamReader(Prefix, expectedEmail, ToStandardFormat);
            var wasSent = _sut.Execute(Prefix, sendStream, receiveStreamReader);

            Assert.IsTrue(wasSent);
            var message = (IMessage)_sut.ExtractMessage();
            Assert.AreEqual(null, message.Content);
            Assert.AreEqual(MessageDirection.Outbound, message.Direction);
            Assert.AreEqual(MessageType.Email, message.MessageType);
            Assert.AreEqual(GmailProviderDescription.Instance, message.Provider);
            Assert.AreEqual(expectedEmail.Subject, message.Subject);
            Assert.AreEqual(expectedEmail.TimeStamp, message.Timestamp);
            Assert.AreEqual(expectedDeepLink, message.DeepLink);
        }

        [Test]
        public void Should_process_multiline_To_inbound_message()
        {
            var expectedEmail = CreateStubMessage();
            expectedEmail.ToAddressLines = new[]
            {
                "Bilbo Baggins <bilbo@lotr.com>, Johnny Cash",
                "<john@cash.com>"
            };
            Assume.That(!expectedEmail.FromAddress.Contains(_accountEmailAddress));
            var expectedDeepLink = CreateDeepLink(_accountEmailAddress, expectedEmail.ThreadId);
            var sendStream = new MemoryStream();
            var receiveStreamReader = CreateReceiveStreamReader(Prefix, expectedEmail, ToStandardFormat);
            var wasSent = _sut.Execute(Prefix, sendStream, receiveStreamReader);

            Assert.IsTrue(wasSent);
            var actual = (IMessage)_sut.ExtractMessage();
            Assert.AreEqual(null, actual.Content);
            Assert.AreEqual(MessageDirection.Inbound, actual.Direction);
            Assert.AreEqual(MessageType.Email, actual.MessageType);
            Assert.AreEqual(GmailProviderDescription.Instance, actual.Provider);
            Assert.AreEqual(expectedEmail.Subject, actual.Subject);
            Assert.AreEqual(expectedEmail.TimeStamp, actual.Timestamp);
            Assert.AreEqual(expectedDeepLink, actual.DeepLink);
        }

        private static readonly object[] DateFormatCases =
        {
            new object[] {"Sat, 8 Feb 2014 23:31:20 +0800",        new DateTimeOffset(2014, 02, 08, 23, 31, 20, TimeSpan.FromHours(8))},
            new object[] {"Thu, 5 Feb 2015 17:34:05 +0000",        new DateTimeOffset(2015, 02, 05, 17, 34, 05, TimeSpan.FromHours(0))},
            new object[] {"Tue, 3 Feb 2015 12:34:49 -0800",        new DateTimeOffset(2015, 02, 03, 12, 34, 49, TimeSpan.FromHours(-8))},
            new object[] {"Tue, 3 Feb 2015 13:38:01 +0100",        new DateTimeOffset(2015, 02, 03, 13, 38, 01, TimeSpan.FromHours(1))},
            new object[] {"Tue, 3 Feb 2015 05:13:55 -0500",        new DateTimeOffset(2015, 02, 03, 05, 13, 55, TimeSpan.FromHours(-5))},
            new object[] {"Tue, 3 Feb 2015 05:13:55 -0500",        new DateTimeOffset(2015, 02, 03, 05, 13, 55, TimeSpan.FromHours(-5))},
            new object[] {"Sat, 08 Feb 2014 07:37:51 -0800",       new DateTimeOffset(2014, 02, 08, 07, 37, 51, TimeSpan.FromHours(-8))},
            new object[] {"Tue, 10 Feb 2015 06:55:13 +0800",       new DateTimeOffset(2015, 02, 10, 06, 55, 13, TimeSpan.FromHours(8))},
            new object[] {"Fri, 19 Sep 2014 12:54:30 +0000",       new DateTimeOffset(2014, 09, 19, 12, 54, 30, TimeSpan.FromHours(0))},
            new object[] {"Thu, 5 Feb 2015 18:21:18 +0100 (CET)",  new DateTimeOffset(2015, 02, 05, 18, 21, 18, TimeSpan.FromHours(1))},
            new object[] {"Thu, 3 Dec 2009 12:12:39 -0800 (PST)",  new DateTimeOffset(2009, 12, 03, 12, 12, 39, TimeSpan.FromHours(-8))},
            new object[] {"Tue, 8 Apr 2008 01:15:41 -0500 (CDT)",  new DateTimeOffset(2008, 04, 08, 01, 15, 41, TimeSpan.FromHours(-5))},
            new object[] {"Sat, 28 Jun 2008 16:03:23 +1000 (EST)", new DateTimeOffset(2008, 06, 28, 16, 03, 23, TimeSpan.FromHours(10))},
            new object[] {"Sat, 24 Jul 2010 01:02:02 -0700 (PDT)", new DateTimeOffset(2010, 07, 24, 01, 02, 02, TimeSpan.FromHours(-7))}
        };

        [TestCaseSource("DateFormatCases")]
        public void Test_timestamp_format(string input, DateTimeOffset expected)
        {
            var expectedEmail = CreateStubMessage();
            expectedEmail.TimeStamp = expected;

            var sendStream = new MemoryStream();
            var receiveStreamReader = CreateReceiveStreamReader(Prefix, expectedEmail, _ => input);
            var wasSent = _sut.Execute(Prefix, sendStream, receiveStreamReader);

            Assert.IsTrue(wasSent);
            var message = _sut.ExtractMessage();
            Assert.AreEqual(expectedEmail.TimeStamp, message.Timestamp);
        }

        

        private static StreamReader CreateReceiveStreamReader(string prefix, Email expectedEmail, Func<DateTimeOffset, string> dateFormatter)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("* 66913 FETCH (X-GM-THRID {0} BODY[HEADER.FIELDS (FROM TO Message-ID Subject Date X-GM-THRID)] {{256}}", expectedEmail.ThreadId));
            sb.AppendLine(string.Format("Message-Id: <{0}>", expectedEmail.MessageId));
            sb.AppendLine(string.Format("From: {0}", expectedEmail.FromAddress));
            sb.AppendLine(string.Format("Subject: {0}", expectedEmail.Subject));
            var dateString = dateFormatter(expectedEmail.TimeStamp);
            sb.AppendLine(string.Format("Date: {0}", dateString));
            sb.AppendLine(string.Format("To: {0}", String.Join("\r\n ", expectedEmail.ToAddressLines)));
            sb.AppendLine("");
            sb.AppendLine(")");
            sb.AppendLine(prefix + " OK Success");

            byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString().ToCharArray());

            var receiveStream = new MemoryStream(bytes);
            receiveStream.Position = 0;
            var receiveStreamReader = new StreamReader(receiveStream);
            return receiveStreamReader;
        }

        private static FetchMessageOperation Create(ulong messageId, string accountEmailAddress)
        {
            var logger = Substitute.For<ILoggerFactory>();
            //var logger = new ConsoleLoggerFactory();
            return new FetchMessageOperation(messageId, accountEmailAddress, logger);
        }

        private static Email CreateStubMessage()
        {
            var message = new Email
            {
                ThreadId = 1493451527478692916,
                MessageId = "788FC3E5-6F7E-40F9-BBD1-DE481B237D7A@gmail.com",
                FromAddress = "Karen MacCarthy-Farrers <farrers@cmail.com>",
                ToAddressLines = new[] { "Rhys Campston <rhysryancampston@cmail.com>" },
                Subject = "Re: Point Walter Triathlon/Duathlon Results",
                TimeStamp = new DateTimeOffset(2015, 02, 22, 12, 003, 46, TimeSpan.Zero)
            };
            return message;
        }

        private static string CreateDeepLink(string accountEmailAddress, long threadId)
        {
            return string.Format("https://mail.google.com/mail/?authuser={0}#all/{1:x}",
                accountEmailAddress.ToLower(), threadId);
        }

        private static string ToStandardFormat(DateTimeOffset input)
        {
            var dateString = input.ToString("ddd, d MMM yyyy HH:mm:ss K");
            return dateString.Remove(dateString.LastIndexOf(':'), 1);
        }

        private class Email
        {
            public long ThreadId { get; set; }
            public string MessageId { get; set; }
            public string FromAddress { get; set; }
            public string[] ToAddressLines { get; set; }
            public string Subject { get; set; }
            public DateTimeOffset TimeStamp { get; set; }
        }
    }
}