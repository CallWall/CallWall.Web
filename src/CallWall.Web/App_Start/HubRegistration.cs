using System;
using System.Reflection;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;

namespace CallWall.Web
{
    public class HubRegistration
    {
        public static void RegisterHubs(IAppBuilder app, IUnityContainer container, ILogger logger)
        {
            logger.Info("Overriding default JSON serialization settings (PascalCase->camelCase)");
            SetJsonSerializerSettings(container);

            logger.Info("SignalR Hubs registration starting ...");
            var config = new HubConfiguration
            {
                Resolver = new UnitySignalRDependencyResolver(container)
            };

            app.MapSignalR(config);
            logger.Info("SignalR Hubs registered.");
        }

        private static void SetJsonSerializerSettings(IUnityContainer container)
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new SignalRContractResolver();
            var serializer = JsonSerializer.Create(settings);
            //GlobalHost.DependencyResolver.Register(typeof (JsonSerializer), () => serializer);
            container.RegisterInstance(typeof (JsonSerializer), serializer);
        }
    }

    public class SignalRContractResolver : IContractResolver
    {
        private readonly Assembly _signalRAssembly;
        private readonly IContractResolver _camelCaseContractResolver;
        private readonly IContractResolver _defaultContractSerializer;

        public SignalRContractResolver()
        {
            _defaultContractSerializer = new DefaultContractResolver();
            _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
            _signalRAssembly = typeof(Connection).Assembly;
        }

        #region IContractResolver Members

        public JsonContract ResolveContract(Type type)
        {
            if (type.Assembly.Equals(_signalRAssembly))
                return _defaultContractSerializer.ResolveContract(type);

            return _camelCaseContractResolver.ResolveContract(type);
        }

        #endregion
    }
}