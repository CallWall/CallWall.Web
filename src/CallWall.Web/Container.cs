using System;
using System.Linq;
using CallWall.Web.Hubs;
using CallWall.Web.Logging;
using CallWall.Web.Providers;
using CallWall.Web.Unity;
using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;

namespace CallWall.Web
{
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
            new LoggerFactory().CreateLogger(typeof(Bootstrapper)).Trace("Registering types");
            container.RegisterType<ILoggerFactory, LoggerFactory>();
            container.RegisterType<ISessionProvider, SessionProvider>();
            container.RegisterType<IAuthenticationProviderGateway, AuthenticationProviderGateway>();
            RegisterHubs(container);
            
            //HACK: Register Fakes. Move to fakes module. (Fix Fakes Module and Build target) -LC
            container.RegisterType<IObservableHubDataProvider<CalendarEntry>, HubFakeDataProvider>();
            container.RegisterType<IObservableHubDataProvider<Message>, HubFakeDataProvider>();
            container.RegisterType<IObservableHubDataProvider<GalleryAlbum>, HubFakeDataProvider>();
            container.RegisterType<IObservableHubDataProvider<ContactCollaboration>, HubFakeDataProvider>();
            InitialiseModules(container);
        }

        private static void RegisterHubs(IUnityContainer container)
        {
            var types = typeof(ContactSummariesHub).Assembly.GetTypes();
            foreach (var hub in types.Where(IsAHub))
            {
                container.RegisterType(hub);
            }
        }

        private static bool IsAHub(Type x)
        {
            return !x.IsAbstract &&
                !x.IsInterface &&
                typeof(Hub).IsAssignableFrom(x);
        }

        private static void InitialiseModules(IUnityContainer container)
        {
            var typeRegistry = new TypeRegistry(container);

            var moduleConfig = CallWallModuleSection.GetConfig();

            var modules = from moduleType in moduleConfig.Modules.Cast<ModuleElement>().Select(m => m.Type)
                          select (IModule)Activator.CreateInstance(moduleType);

            var logger = new LoggerFactory().CreateLogger(typeof(Bootstrapper));
            logger.Trace("Initialising modules...");
            foreach (var module in modules)
            {
                logger.Trace("Initialising module : {0}", module.GetType().Name);
                module.Initialise(typeRegistry);
            }
            logger.Trace("Modules Initialised");
        }

        public static bool IsModule(Type type)
        {
            var moduleType = typeof(IModule);
            return type.IsPublic
                   && !type.IsAbstract
                   && moduleType.IsAssignableFrom(type);
        }
    }
}