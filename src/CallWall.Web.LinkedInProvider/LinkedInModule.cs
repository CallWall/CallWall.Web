using CallWall.Web.LinkedInProvider.Auth;
using CallWall.Web.LinkedInProvider.Contacts;

namespace CallWall.Web.LinkedInProvider
{
    public sealed class LinkedInModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterType<IContactsProvider, LinkedInContactsProvider>("LinkedInContactsProvider");
            registry.RegisterType<IAccountAuthentication, LinkedInAuthentication>("LinkedInAuthentication");
        }
    }
}