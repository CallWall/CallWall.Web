using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using $rootnamespace$.Hubs;
using $rootnamespace$.Logging;
using $rootnamespace$.Providers;
using $rootnamespace$.Unity;
using Microsoft.Practices.Unity;
using Unity.Mvc4;

namespace $rootnamespace$
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

    public sealed class TypeRegistry : ITypeRegistry
    {
        private readonly IUnityContainer _container;

        public TypeRegistry(IUnityContainer container)
        {
            _container = container;
        }

        public void RegisterType<TFrom, TTo>() where TTo : TFrom
        {
            _container.RegisterType<TFrom, TTo>();
        }

        public void RegisterType<TFrom, TTo>(string name) where TTo : TFrom
        {
            _container.RegisterType<TFrom, TTo>(name);
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
            new LoggerFactory().CreateLogger(typeof(Bootstrapper)).Trace("Registering types");
            container.RegisterType<ILoggerFactory, LoggerFactory>();
            container.RegisterType<Providers.ISecurityProvider, Providers.SecurityProvider>();
            container.RegisterType<ContactsHub>(new InjectionFactory(CreateContactsHub));

            InitialiseModules(container);
        }

        private static void InitialiseModules(IUnityContainer container)
        {
            var typeRegistry = new TypeRegistry(container);

            var providersPath = GetProvidersPath();

            //Look for implementations of IModule in each assembly from providers folder
            var modules = from file in Directory.EnumerateFiles(providersPath)
                    where file.EndsWith(".dll")
                    let assembly = Assembly.LoadFile(file)
                    from type in assembly.GetTypes()
                    where IsModule(type)
                              select (IModule)Activator.CreateInstance(type);
                    
            foreach (var module in modules)
            {
                module.Initialise(typeRegistry);
            }
        }

        private static string GetProvidersPath()
        {
            var webRoot = System.AppDomain.CurrentDomain.BaseDirectory;
            var providersPath = Path.Combine(webRoot, @"bin\Providers");
            return providersPath;
        }

        public static bool IsModule(Type type)
        {
            var moduleType = typeof (IModule);
            return type.IsPublic
                   && !type.IsAbstract
                   && moduleType.IsAssignableFrom(type);
        }

        private static object CreateContactsHub(IUnityContainer container)
        {
            //TODO: Can this just become container.Resolve<ContactsHub>();
            var hub = new ContactsHub(container.Resolve<IContactsProvider>(), container.Resolve<ISecurityProvider>(), container.Resolve<ILoggerFactory>());
            return hub;
        }
    }
}