﻿using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Authentication;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Providers.Contacts;
using CallWall.Web.GoogleProvider.Providers.Gmail.Imap;

namespace CallWall.Web.GoogleProvider.Providers.Gmail
{
    public sealed class GmailCommunicationQueryProvider : ICommunicationQueryProvider
    {
        private readonly Func<IImapClient> _imapClientFactory;
        private readonly IAuthorizationTokenProvider _authorization;
        private readonly ICurrentGoogleUserProvider _contactQueryProvider;
        private readonly ILogger _logger;

        public GmailCommunicationQueryProvider(Func<IImapClient> imapClientFactory, IAuthorizationTokenProvider authorization, ICurrentGoogleUserProvider contactQueryProvider, ILoggerFactory loggerFactory)
        {
            _imapClientFactory = imapClientFactory;
            _authorization = authorization;
            _contactQueryProvider = contactQueryProvider;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public IObservable<IMessage> LoadMessages(IProfile activeProfile)
        {
            return (from token in _authorization.RequestAccessToken(ResourceScope.Gmail).ToObservable()
                   from message in SearchImap(activeProfile, token)
                   select message)
                   .Log(_logger, "LoadMessages");
        }

        private IObservable<IMessage> SearchImap(IProfile activeProfile, string accessToken)
        {
            return Observable.Using(_imapClientFactory, imapClient => SearchImap(imapClient, activeProfile, accessToken));
        }

        private IObservable<IMessage> SearchImap(IImapClient imapClient, IProfile activeProfile, string accessToken)
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
                        let queryText = ToSearchQuery(activeProfile)
                        from emailIds in imapClient.FindEmailIds(queryText)
                        from email in imapClient.FetchEmailSummaries(emailIds.Reverse().Take(15), currentUser.EmailAddresses)
                        select email;

            return query.TakeUntil(_contactQueryProvider.CurrentUser().Where(user => user != null).Skip(1));
        }

        private static string ToSearchQuery(IProfile activeProfile)
        {
            return string.Join(" OR ", activeProfile.Identifiers.Select(id => string.Format("\"{0}\"", id.Value)));
        }
    }
}