using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Authentication;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Providers.Gmail.Imap;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider.Providers.Gmail
{
    public sealed class GmailCommunicationQueryProvider : ICommunicationProvider
    {
        private readonly Func<IImapClient> _imapClientFactory;
        private readonly ILogger _logger;

        public GmailCommunicationQueryProvider(Func<IImapClient> imapClientFactory, ILoggerFactory loggerFactory)
        {
            _imapClientFactory = imapClientFactory;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public IObservable<IMessage> GetMessages(User user, string[] contactKeys)
        {
            return GetAuthorizedGMailAccounts(user)
                .Select(acc => SearchImap(contactKeys, acc))
                .Merge()
                .Log(_logger, "LoadMessages");
        }

        private static IEnumerable<IAccount> GetAuthorizedGMailAccounts(User user)
        {
            //TODO: Enable the getting of a new token, and persisting it to the ES -LC
            return user.Accounts
                .Where(acc => !acc.CurrentSession.HasExpired())
                .Where(acc => acc.Provider == "Google")
                .Where(acc => acc.CurrentSession.AuthorizedResources.Contains(ResourceScope.Gmail.Resource));
        }

        private IObservable<IMessage> SearchImap(IEnumerable<string> contactKeys, IAccount account)
        {
            return Observable.Using(_imapClientFactory, imapClient => SearchImap(imapClient, contactKeys, account));
        }

        private IObservable<IMessage> SearchImap(IImapClient imapClient, IEnumerable<string> contactKeys, IAccount account)
        {
            var query = from isConnected in imapClient.Connect("imap.gmail.com", 993)
                                         .Select(isConnected =>
                                         {
                                             if (isConnected) return true;
                                             throw new IOException("Failed to connect to Gmail IMAP server.");
                                         })
                        from isAuthenticated in imapClient.Authenticate(account.AccountId, account.CurrentSession.AccessToken)
                                                          .Select(isAuthenticated =>
                                                          {
                                                              if (isAuthenticated) return true;
                                                              throw new AuthenticationException("Failed to authenticate for Gmail search.");
                                                          })
                        from isSelected in imapClient.SelectFolder("[Gmail]/All Mail")
                        let queryText = ToSearchQuery(contactKeys)
                        from emailIds in imapClient.FindEmailIds(queryText)
                        from email in imapClient.FetchEmailSummaries(emailIds.Reverse().Take(15), account.Handles.Where(ch => ch.HandleType == ContactHandleTypes.Email).Select(ch => ch.Handle))
                        select email;

            return query;
        }

        private static string ToSearchQuery(IEnumerable<string> contactKeys)
        {
            return string.Join(" OR ", contactKeys.Select(id => string.Format("\"{0}\"", id)));
        }
    }
}
