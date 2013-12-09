using CallWall.Web.GoogleProvider.Contacts;

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