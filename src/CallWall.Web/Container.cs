using System;
using System.Linq;
using CallWall.Web.Hubs;
using CallWall.Web.Logging;
using CallWall.Web.Providers;
using CallWall.Web.Unity;
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
            container.RegisterType<ContactSummariesHub>();
            container.RegisterType<ContactProfileHub>();

            InitialiseModules(container);
        }

        private static void InitialiseModules(IUnityContainer container)
        {
            var typeRegistry = new TypeRegistry(container);

            var moduleConfig = CallWallModuleSection.GetConfig();
            var modules = from moduleType in moduleConfig.Modules.Cast<ModuleElement>().Select(m => m.Type)
                          select (IModule)Activator.CreateInstance(moduleType);

            foreach (var module in modules)
            {
                module.Initialise(typeRegistry);
            }
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