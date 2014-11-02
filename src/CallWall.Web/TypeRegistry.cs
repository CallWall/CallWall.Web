using Microsoft.Practices.Unity;

namespace CallWall.Web
{
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

        public void RegisterSingleton<TFrom, TTo>() where TTo : TFrom
        {
            _container.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager());
        }
    }
}