
using System.Threading.Tasks;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Contacts;
using CallWall.Web.Providers;

namespace CallWall.Web.EventStore
{
    public sealed class EventStoreModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterSingleton<IEventStoreClient, EventStoreClient>();
            registry.RegisterSingleton<IEventStoreConnectionFactory, EventStoreConnectionFactory>();
            registry.RegisterSingleton<Accounts.IAccountContactRefresher, Accounts.AccountContactRefresher>();
            registry.RegisterSingleton<Contacts.IAccountContactsFactory, Contacts.AccountContactsFactory>();
            registry.RegisterSingleton<Contacts.IUserContactRepository, Contacts.UserContactRepository>();
            registry.RegisterType<IAccountContactProvider, Contacts.EventStoreAccountContactProvider>("EventStoreAccountContactProvider");


            registry.RegisterSingleton<IUserRepository, Users.UserRepository>();
            registry.RegisterSingleton<IAccountFactory, Accounts.AccountFactory>();
            registry.RegisterSingleton<IContactRepository, ContactRepository>();
   
            registry.RegisterType<IProcess, EventStoreProcess>("EventStoreProcess");
        }
    }

    //TODO: Consider making some of these out of process services (AccountContactSynchronizationService & UserContactSynchronizationService). -LC
    public sealed class EventStoreProcess : IProcess
    {
        private readonly Contacts.AccountContactSynchronizationService _accountContactSynchronizationService;
        private readonly Contacts.UserContactSynchronizationService _userContactSynchronizationService;
        private readonly IUserRepository _userRepository;

        public EventStoreProcess(Contacts.AccountContactSynchronizationService accountContactSynchronizationService,
             Contacts.UserContactSynchronizationService userContactSynchronizationService,
             IUserRepository userRepository)
        {
            _accountContactSynchronizationService = accountContactSynchronizationService;
            _userContactSynchronizationService = userContactSynchronizationService;
            _userRepository = userRepository;
        }

        public async Task Run()
        {
            await Task.WhenAll(
                _accountContactSynchronizationService.Run(),
                _userContactSynchronizationService.Run(),
                _userRepository.Run());
        }
    }
}