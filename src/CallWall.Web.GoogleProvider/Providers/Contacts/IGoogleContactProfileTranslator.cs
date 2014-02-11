using System;
using System.Collections.Generic;
using System.Linq;

namespace CallWall.Web.GoogleProvider.Providers.Contacts
{
    public interface IGoogleContactProfileTranslator
    {
        IGoogleContactProfile Translate(string response, string accessToken);
        IGoogleContactProfile AddTags(IGoogleContactProfile contactProfile, string response);
        GoogleUser GetUser(string response);
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
}