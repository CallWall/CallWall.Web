using CallWall.Web.Domain;
using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Providers.Contacts;
using CallWall.Web.GoogleProvider.Providers.Gmail;
using CallWall.Web.GoogleProvider.Providers.Gmail.Imap;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider
{
    public sealed class GoogleModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterType<IAccountContactProvider, GoogleAccountContactProvider>("GoogleContactsProvider");
            registry.RegisterType<IAccountAuthentication, GoogleAuthentication>("GoogleAuthentication");

            registry.RegisterType<IImapClient, ImapClient>();

            registry.RegisterType<IGoogleAccountProvider, GoogleAccountProvider>();
            registry.RegisterType<ICommunicationProvider, GmailCommunicationQueryProvider>();

            registry.RegisterType<IImapDateTranslator, ImapDateTranslator>();
            registry.RegisterType<IGoogleContactProfileTranslator, GoogleContactProfileTranslator>();
            registry.RegisterType<IAccountTranslator, AccountTranslator>();
        }
    }
}