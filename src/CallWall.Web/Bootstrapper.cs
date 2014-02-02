using System.Web.Mvc;
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
}