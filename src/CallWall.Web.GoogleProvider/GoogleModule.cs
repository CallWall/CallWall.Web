using CallWall.Web.GoogleProvider.Auth;
using CallWall.Web.GoogleProvider.Contacts;
using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProvider
{
    public sealed class GoogleModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterType<IContactsProvider, GoogleContactsProvider>("GoogleContactsProvider");
            registry.RegisterType<IAccountAuthentication, GoogleAuthentication>("GoogleAuthentication");
        }
    }
}