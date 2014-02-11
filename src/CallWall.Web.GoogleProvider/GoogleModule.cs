using CallWall.Web.Contracts;
using CallWall.Web.Contracts.Communication;
using CallWall.Web.Contracts.Contact;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Contacts;
using CallWall.Web.GoogleProvider.Providers.Contacts;
using CallWall.Web.GoogleProvider.Providers.Gmail;
using CallWall.Web.GoogleProvider.Providers.Gmail.Imap;

namespace CallWall.Web.GoogleProvider
{
    public sealed class GoogleModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterType<IContactsProvider, GoogleContactsProvider>("GoogleContactsProvider");
            registry.RegisterType<IAccountAuthentication, GoogleAuthentication>("GoogleAuthentication");

            registry.RegisterType<ICommunicationQueryProvider, GmailCommunicationQueryProvider>();
            registry.RegisterType<IImapClient, ImapClient>();
            registry.RegisterType<IImapDateTranslator, ImapDateTranslator>();
            registry.RegisterType<IGoogleContactProfileTranslator, Providers.Contacts.GoogleContactProfileTranslator>();
            registry.RegisterType<IContactQueryProvider, GoogleContactQueryProvider>(); 
            registry.RegisterType<ICurrentGoogleUserProvider, GoogleContactQueryProvider>();
            registry.RegisterType<IAuthorizationTokenProvider, GoogleAuthorizationTokenProvider>();
        }
    }
}