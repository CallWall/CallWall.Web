using System;
using System.Linq;
using System.Threading.Tasks;
using CallWall.Web.Contracts;
using CallWall.Web.Domain;
using CallWall.Web.Http;
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
            new LoggerFactory().CreateLogger(typeof(Bootstrapper)).Trace("Container creating..");
            var container = new UnityContainer();

            container.AddNewExtension<GenericSupportExtension>();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();    
            RegisterTypes(container);

            new LoggerFactory().CreateLogger(typeof(Bootstrapper)).Trace("Container created.");
            return container;
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            new LoggerFactory().CreateLogger(typeof(Bootstrapper)).Trace("Registering types");
            container.RegisterType<ILoggerFactory, LoggerFactory>();
            container.RegisterType<ILoginProvider, LoginProvider>();
            //Core
            container.RegisterType<IHttpClient, HttpClient>();
            container.RegisterType<ISchedulerProvider, SchedulerProvider>();

            container.RegisterType<IAuthenticationProviderGateway, AuthenticationProviderGateway>();
            container.RegisterType<IAccountFactory, AccountFactory>();
            RegisterHubs(container);
            
            InitialiseModules(container);
        }

        private static void RegisterHubs(IUnityContainer container)
        {
            new LoggerFactory().CreateLogger(typeof(Bootstrapper)).Trace("Registering types");
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
            var logger = new LoggerFactory().CreateLogger(typeof(Bootstrapper));
            logger.Trace("Initializing modules");
            var typeRegistry = new TypeRegistry(container);

            var moduleConfig = CallWallModuleSection.GetConfig();

            var modules = from moduleType in moduleConfig.Modules.Cast<ModuleElement>().Select(m => m.Type)
                          select (IModule)Activator.CreateInstance(moduleType);

            
            logger.Trace("Initializing modules...");
            foreach (var module in modules)
            {
                logger.Trace("Initializing module : {0}", module.GetType().Name);
                module.Initialise(typeRegistry);
            }
            logger.Trace("Modules Initialized");


            logger.Info("Starting processes");
            var processes = container.ResolveAll<IProcess>();
            Task.WhenAll(processes.Select(p => p.Run()))
                .Wait();
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