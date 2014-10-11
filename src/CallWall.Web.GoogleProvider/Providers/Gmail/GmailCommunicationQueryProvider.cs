using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Authentication;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Providers.Contacts;
using CallWall.Web.GoogleProvider.Providers.Gmail.Imap;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider.Providers.Gmail
{
    public sealed class GmailCommunicationQueryProvider : ICommunicationProvider
    {
        private readonly Func<IImapClient> _imapClientFactory;
        private readonly ICurrentGoogleUserProvider _contactQueryProvider;
        private readonly ILogger _logger;

        public GmailCommunicationQueryProvider(Func<IImapClient> imapClientFactory, ICurrentGoogleUserProvider contactQueryProvider, ILoggerFactory loggerFactory)
        {
            _imapClientFactory = imapClientFactory;
            _contactQueryProvider = contactQueryProvider;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public IObservable<IMessage> GetMessages(IEnumerable<ISession> session, string[] contactKeys)
        {
            return (from token in GetGMailAccessToken(session)
                    from message in SearchImap(contactKeys, token)
                    select message)
                  .Log(_logger, "LoadMessages");
        }

        private static IObservable<string> GetGMailAccessToken(IEnumerable<ISession> session)
        {
            return session.Where(s => s.AuthorizedResources.Contains(ResourceScope.Gmail.Resource))
                    .Select(s => s.AccessToken)
                    .ToObservable();
        }

        private IObservable<IMessage> SearchImap(IEnumerable<string> contactKeys, string accessToken)
        {
            return Observable.Using(_imapClientFactory, imapClient => SearchImap(imapClient, contactKeys, accessToken));
        }

        private IObservable<IMessage> SearchImap(IImapClient imapClient, IEnumerable<string> contactKeys, string accessToken)
        {
            var query = from token in Observable.Return(accessToken)
                        from currentUser in _contactQueryProvider.CurrentUser()
                                                                 .Where(user => user != null)
                                                                 .Take(1)
                        from isConnected in imapClient.Connect("imap.gmail.com", 993)
                                         .Select(isConnected =>
                                         {
                                             if (isConnected) return true;
                                             throw new IOException("Failed to connect to Gmail IMAP server.");
                                         })
                        from isAuthenticated in imapClient.Authenticate(currentUser.Id, token)
                                                          .Select(isAuthenticated =>
                                                          {
                                                              if (isAuthenticated) return true;
                                                              throw new AuthenticationException("Failed to authenticate for Gmail search.");
                                                          })
                        from isSelected in imapClient.SelectFolder("[Gmail]/All Mail")
                        let queryText = ToSearchQuery(contactKeys)
                        from emailIds in imapClient.FindEmailIds(queryText)
                        from email in imapClient.FetchEmailSummaries(emailIds.Reverse().Take(15), currentUser.EmailAddresses)
                        select email;

            return query.TakeUntil(_contactQueryProvider.CurrentUser().Where(user => user != null).Skip(1));
        }

        private static string ToSearchQuery(IEnumerable<string> contactKeys)
        {
            return string.Join(" OR ", contactKeys.Select(id => string.Format("\"{0}\"", id)));
        }
    }
}
