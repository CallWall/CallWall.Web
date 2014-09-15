
namespace CallWall.Web.EventStore
{

    //TODO: This is just what could be used. Not used/run yet -LC
    public sealed class EventStoreModule : IModule
    {
        public void Initialise(ITypeRegistry registry)
        {
            registry.RegisterType<IEventStoreClient, EventStoreClient>();
            registry.RegisterType<IEventStoreConnectionFactory, EventStoreConnectionFactory>();
            registry.RegisterType<Accounts.IAccountContactRefresher, Accounts.AccountContactRefresher>();
            registry.RegisterType<Contacts.IAccountContactsFactory, Contacts.AccountContactsFactory>();
            registry.RegisterType<Contacts.IUserContactRepository, Contacts.UserContactRepository>();
            registry.RegisterType<Users.IUserRepository, Users.UserRepository>();         
   
            registry.RegisterType<IProcess, EventStoreProcess>("EventStoreProcess");
        }
    }

    //TODO: Consider making some of these out of process services (AccountContactSynchronizationService & UserContactSynchronizationService). -LC
    public sealed class EventStoreProcess : IProcess
    {
        private readonly Contacts.AccountContactSynchronizationService _accountContactSynchronizationService;
        private readonly Contacts.UserContactSynchronizationService _userContactSynchronizationService;
        private readonly Users.IUserRepository _userRepository;

        public EventStoreProcess(Contacts.AccountContactSynchronizationService accountContactSynchronizationService,
             Contacts.UserContactSynchronizationService userContactSynchronizationService,
             Users.IUserRepository userRepository)
        {
            _accountContactSynchronizationService = accountContactSynchronizationService;
            _userContactSynchronizationService = userContactSynchronizationService;
            _userRepository = userRepository;
        }

        public void Run()
        {
            _accountContactSynchronizationService.Run();
            _userContactSynchronizationService.Run();
            _userRepository.Run();
        }
    }
}