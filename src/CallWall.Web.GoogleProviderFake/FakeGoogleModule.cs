using CallWall.Web.Providers;

namespace CallWall.Web.GoogleProviderFake
{
    public sealed class FakeGoogleModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterType<IContactsProvider, FakeGoogleContactsProvider>();
            registry.RegisterType<IAccountAuthentication, FakeGoogleAuthentication>("FakeGoogleAuthentication");
        }
    }
}
