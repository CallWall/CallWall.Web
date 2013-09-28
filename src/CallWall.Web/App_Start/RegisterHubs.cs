using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using CallWall.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.Practices.Unity;

//[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(RegisterHubs), "Start")]

namespace CallWall.Web
{
    public static class RegisterHubs
    {
        public static void Start(IUnityContainer container)
        {
            // Register the default hubs route: ~/signalr
            //var container = Container.Create();

            var config = new HubConfiguration
                {
                    Resolver = new UnitySignalRDependencyResolver(container)
                };

            RouteTable.Routes.MapHubs(config);
        }
    }

    internal class UnitySignalRDependencyResolver : DefaultDependencyResolver
    {
        private readonly IUnityContainer _container;
        public UnitySignalRDependencyResolver(IUnityContainer container)
        {
            if(container==null) throw new ArgumentNullException("container");
            _container = container;
        }

        public override object GetService(Type serviceType)
        {
            var isRegistered = _container.IsRegistered(serviceType);
            object result = null;
            if (isRegistered) 
                result = _container.Resolve(serviceType);
            else 
                result =  base.GetService(serviceType);

            return result;
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.ResolveAll(serviceType).Concat(base.GetServices(serviceType));
        }
        public override void Register(Type serviceType, Func<object> activator)
        {
            Console.WriteLine("Register({0}, activator)", serviceType.Name);
            base.Register(serviceType, activator);
        }
        public override void Register(Type serviceType, IEnumerable<Func<object>> activators)
        {
            Console.WriteLine("Register({0}, activators)", serviceType.Name);
            base.Register(serviceType, activators);
        }
    }
}
