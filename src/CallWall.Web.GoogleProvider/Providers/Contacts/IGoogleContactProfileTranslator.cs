using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.Http;

namespace CallWall.Web.GoogleProvider.Providers.Contacts
{
    public interface IGoogleContactProfileTranslator
    {
        IGoogleContactProfile Translate(string response, string accessToken);
        IGoogleContactProfile AddTags(IGoogleContactProfile contactProfile, string response);
        GoogleUser GetUser(string response);
        IAccount TranslateToAccount(string response, ISession session);
    }

    public interface ICurrentGoogleUserProvider
    {
        IObservable<GoogleUser> CurrentUser();
    }

    public sealed class GoogleUser
    {
        public GoogleUser(string currentUser, IEnumerable<string> emailAddresses)
        {
            Id = currentUser;
            EmailAddresses = emailAddresses.ToArray();
        }
        public string Id { get; private set; }
        public string[] EmailAddresses { get; private set; }
    }

    public interface IGoogleAccountProvider
    {
        Task<IAccount> CreateAccount(ISession session);
    }

    class GoogleAccountProvider : IGoogleAccountProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly IGoogleContactProfileTranslator _contactTranslator;

        public GoogleAccountProvider(IHttpClient httpClient, IGoogleContactProfileTranslator contactTranslator)
        {
            _httpClient = httpClient;
            _contactTranslator = contactTranslator;
        }

        public async Task<IAccount> CreateAccount(ISession session)
        {
            var request = HttpParams.CreateRequestParams(session.AccessToken);
            var response = await _httpClient.GetResponse(request).ToTask();
            return _contactTranslator.TranslateToAccount(response, session);
        }
    }
}