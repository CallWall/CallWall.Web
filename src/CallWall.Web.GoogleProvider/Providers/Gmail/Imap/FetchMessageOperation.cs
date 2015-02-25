using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CallWall.Web.Contracts.Communication;

namespace CallWall.Web.GoogleProvider.Providers.Gmail.Imap
{
    //http://tools.ietf.org/search/rfc3501#page-54
    internal sealed class FetchMessageOperation : ImapOperationBase
    {
        private readonly string _accountEmailAddress;
        private readonly string _command;
        private readonly IImapDateTranslator _dateTranslator = new ImapDateTranslator();
        private readonly GmailDeepLinkParser _deepLinkParser = new GmailDeepLinkParser();

        public FetchMessageOperation(ulong messageId, string accountEmailAddress, ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _command = string.Format("FETCH {0} (BODY.PEEK[HEADER.FIELDS (FROM TO Message-ID Subject Date)] X-GM-THRID)", messageId);
            //TODO: Run through custom gmail address normalizer (googlemail->gmail, removed '+', '.' etc.)
            _accountEmailAddress = accountEmailAddress.ToLowerInvariant();
        }

        protected override string Command
        {
            get { return _command; }
        }

        public GmailEmail ExtractMessage()
        {
            var isDateSet = false;
            var isSubjectSet = false;
            var date = DateTimeOffset.MinValue;
            string subject = null;
            var direction = MessageDirection.Inbound;

            var kvp = new Dictionary<string, string>();
            var lastKey = string.Empty;

            var deepLink = _deepLinkParser.ParseDeepLink(ResponseLines.First.Value, _accountEmailAddress);
            
            foreach (var line in ResponseLines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("*")) 
                    continue;
                if (string.Equals(line, ")"))
                    break;
                if (line.StartsWith(" "))
                {
                    kvp[lastKey] += line;
                }
                else
                {
                    var indexOf = line.IndexOf(":", StringComparison.Ordinal);
                    var key = line.Substring(0, indexOf);
                    kvp[key] = line.Substring(indexOf + 2);
                    lastKey = key;
                }
            }
            if(kvp.ContainsKey("Date"))
            {
                date = _dateTranslator.Translate(kvp["Date"]);
                isDateSet = true; 
            }
            if (kvp.ContainsKey("Subject"))
            {
                subject = kvp["Subject"];
                isSubjectSet = true;
            }
            if (kvp.ContainsKey("From"))
            {
                var fromAddress = kvp["From"].ToLowerInvariant();
                if (fromAddress.Contains(_accountEmailAddress))
                {
                    direction = MessageDirection.Outbound;
                }
            } 
           
            /*
[<--]Date: Thu, 27 Sep 2012 09:13:40 +0100
[<--]Message-ID: <CAPLQusCjGquLW-shZiFkSEWnDZkWO1j+Kc6_aGvtpJybqiRdFQ@mail.gmail.com>
[<--]Subject: Erynne's details
[<--]From: Lee Campbell <lee.ryan.campbell@gmail.com>
[<--]To: Marcus Whitworth <hello@marcuswhitworth.com>

             */
            if (isDateSet && isSubjectSet)
            {
                return new GmailEmail(date, direction, subject, deepLink);
            }

            return null;
        }


    }
}