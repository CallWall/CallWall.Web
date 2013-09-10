using System.Web.Mvc;
using CallWall.Web.Hubs;
using CallWall.Web.Providers;
using CallWall.Web.Providers.Google;
using CallWall.Web.Unity;
using Microsoft.Practices.Unity;
using Unity.Mvc4;

namespace CallWall.Web
{
    public static class Bootstrapper
    {
        public static IUnityContainer Initialise()
        {
            var container = Container.Create();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            return container;
        }
    }

    public static class Container
    {
        public static IUnityContainer Create()
        {
            var container = new UnityContainer();

            container.AddNewExtension<GenericSupportExtension>();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();    
            RegisterTypes(container);

            return container;
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<Providers.ISecurityProvider, Providers.SecurityProvider>();

            //GOOGLE
            container.RegisterType<IContactsProvider, GoogleContactsProvider>();
            container.RegisterType<IAccountAuthentication, GoogleAuthentication>("GoogleAuthentication");


            container.RegisterType<ContactsHub>(new InjectionFactory(CreateContactsHub));
        }

        private static object CreateContactsHub(IUnityContainer arg)
        {
            var hub = new ContactsHub(arg.Resolve<IContactsProvider>(), arg.Resolve<ISecurityProvider>());
            return hub;
        }
    }
}