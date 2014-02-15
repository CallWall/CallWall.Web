using System;

namespace CallWall.Web.GoogleProvider.Providers.Gmail.Imap
{
    public interface IImapDateTranslator
    {
        DateTimeOffset Translate(string imapDate);
    }
}