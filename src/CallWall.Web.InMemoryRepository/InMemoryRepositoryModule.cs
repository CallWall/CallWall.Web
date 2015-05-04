using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.InMemoryRepository
{
    public sealed class InMemoryRepositoryModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterSingleton<IUserRepository, UserRepository>();
            registry.RegisterSingleton<IContactFeedRepository, ContactFeedRepository>();

            registry.RegisterType<IProcess, InMemoryRepositoryProcess>("InMemoryRepositoryProcess");
        }
    }

    public sealed class InMemoryRepositoryProcess : IProcess
    {
        private readonly IContactFeedRepository _contactFeedRepository;

        public InMemoryRepositoryProcess(IContactFeedRepository contactFeedRepository)
        {
            _contactFeedRepository = contactFeedRepository;
        }

        public async Task Run()
        {
            var runnable = _contactFeedRepository as IRunnable;
            if (runnable != null)
                await runnable.Run();
        }
    }
}