using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Hosting;
using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Contact;
using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.Http;


namespace CallWall.Web.GoogleProvider.Providers.Contacts
{
    public sealed class GoogleContactQueryProvider : IContactQueryProvider,ICurrentGoogleUserProvider
    {
        private readonly IAuthorizationTokenProvider _authorization;
        private readonly IHttpClient _httpClient;
        private readonly IGoogleContactProfileTranslator _translator;
        private readonly ILogger _logger;
        private readonly BehaviorSubject<GoogleUser> _currentUser  = new BehaviorSubject<GoogleUser>(null);

        public GoogleContactQueryProvider(IAuthorizationTokenProvider authorization,
                                          IHttpClient httpClient,
                                          IGoogleContactProfileTranslator translator,
                                          ILoggerFactory loggerFactory)
        {
            //TODO: Get auth from the user, not from a service now. -LC
            _authorization = authorization;
            _httpClient = httpClient;
            _translator = translator;
            _logger = loggerFactory.CreateLogger(GetType());
            RetrieveCurentUser().Subscribe(_currentUser);//TODO Make this Lazy
        }

        public IObservable<IContactProfile> LoadContact(IProfile activeProfile)
        {
            return from contact in activeProfile.Identifiers.Select(LoadContact).Merge() //Fires many requests off to hopefully get a matching contact
                   from taggedContact in EnrichTags(contact)
                   select taggedContact;
        }
        public IObservable<GoogleUser> CurrentUser()
        {
            return _currentUser.AsObservable();
        }

        private IObservable<GoogleUser> RetrieveCurentUser()
        {
            //Replace with something like ...
            //var user = new User(Guid.Empty, null, null);
            //var accessTokens = user.Accounts.Where(acc => acc.Provider == "Google")
            //    .Where(acc => acc.CurrentSession != null && !acc.CurrentSession.HasExpired())
            //    .Where(acc => acc.CurrentSession.AuthorizedResources.Contains(ResourceScope.Contacts.Resource))
            //    .Select(acc => acc.CurrentSession.AccessToken);
            

            var query = from accessToken in _authorization.RequestAccessToken(ResourceScope.Contacts).ToObservable()
                from request in HttpParams.CreateRequestParams(accessToken).AsObservable()
                from response in _httpClient.GetResponse(request)
                select _translator.GetUser(response);
            return query;
        }

        private IObservable<IGoogleContactProfile> LoadContact(IPersonalIdentifier personalIdentifier)
        {
            return (
                       from accessToken in _authorization.RequestAccessToken(ResourceScope.Contacts).ToObservable()//.Log(_logger, "2")
                       from request in HttpParams.CreateRequestParams(personalIdentifier, accessToken).AsObservable().Log(_logger, "3")
                       from response in _httpClient.GetResponse(request)
                       select _translator.Translate(response, accessToken)
                   ).Where(profile => profile != null)
                .Log(_logger, string.Format("LoadContact({0})", personalIdentifier.Value))
                .Take(1);
        }

        private IObservable<IGoogleContactProfile> EnrichTags(IGoogleContactProfile contactProfile)
        {
            //TODO: This should fetch any extra pages of groups//TODO: The groups can be cached as they are related to the logged in user. I would imagine that we can safely cache for 1minute.
            return (
                       from accessToken in _authorization.RequestAccessToken(ResourceScope.Contacts).ToObservable()
                       from request in HttpParams.CreateContactGroupRequestParams(accessToken).AsObservable()
                       from response in _httpClient.GetResponse(request)
                       select _translator.AddTags(contactProfile, response)
                   )
                .Log(_logger, string.Format("EnrichTags({0})", contactProfile.FullName))
                .Take(1);
        }
    }
}
