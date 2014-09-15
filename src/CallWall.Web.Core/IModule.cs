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
        void Run();
    }
}
