using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CallWall.Web
{
    public interface IModule
    {
        //Need to be able to Register singleton, instance, and composite
        void Initialise(ITypeRegistry registry);
    }

    public interface IProcess
    {
        Task Run();
    }
}
