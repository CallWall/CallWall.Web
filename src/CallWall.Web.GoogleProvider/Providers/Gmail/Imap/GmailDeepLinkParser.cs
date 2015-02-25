using System.Text.RegularExpressions;

namespace CallWall.Web.GoogleProvider.Providers.Gmail.Imap
{
    public sealed class GmailDeepLinkParser
    {
        private static readonly Regex ThreadIdRegex = new Regex(@"X-GM-THRID (?<threadId>\d+) ", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string ParseDeepLink(string line, string accountEmailAddress)
        {
            string threadId;
            if (TryParseThreadId(line, out threadId))
            {
                return string.Format("https://mail.google.com/mail/?authuser={0}#all/{1}", accountEmailAddress, threadId);
            }
            return null;
        }

        private static bool TryParseThreadId(string line, out string threadId)
        {
            threadId = null;
            var match = ThreadIdRegex.Match(line);
            if (match.Success)
            {
                threadId = match.Groups["threadId"].Value;
            }
            return match.Success;
        }
    }
}