using CallWall.Web.Domain;
using CallWall.Web.LinkedInProvider.Auth;
using CallWall.Web.LinkedInProvider.Contacts;
using CallWall.Web.Providers;

namespace CallWall.Web.LinkedInProvider
{
    public sealed class LinkedInModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterType<IAccountContactProvider, LinkedInAccountContactProvider>("LinkedInContactsProvider");
            registry.RegisterType<IAccountAuthentication, LinkedInAuthentication>("LinkedInAuthentication");
            registry.RegisterType<ILinkedInAccountProvider, LinkedInAccountProvider>();
        }
    }
}