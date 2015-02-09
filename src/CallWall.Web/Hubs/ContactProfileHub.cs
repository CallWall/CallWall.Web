using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using CallWall.Web.Domain;
using CallWall.Web.Providers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace CallWall.Web.Hubs
{
    [HubName("contactProfile")]
    public class ContactProfileHub : Hub
    {
        private readonly SerialDisposable _subscription = new SerialDisposable();
        private readonly IContactRepository _contactRepository;
        private readonly ILoginProvider _loginProvider;
        private readonly ILogger _logger;

        public ContactProfileHub(IContactRepository contactsProviders, ILoginProvider loginProvider, ILoggerFactory loggerFactory)
        {
            _contactRepository = contactsProviders;
            _loginProvider = loginProvider;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task Subscribe(string contactId)
        {
            try
            {
                Debug.Print("ContactProfileHub.Subscribe(...)");


                //TODO: The first thing we should do is get what we already have on the contact from the EventStore.
                //  I suppose we can just add an EventStore implementation of the GetContactDetails
                // NO! 
                // 1) Use the UserRepo (in memory) to match the contactKeys with a contact
                // 2) Return that ContactProfile
                // 3) Use that ContactProfile to then pass to IAccountContactProvider implementations.


                Guid userId = Context.User.UserId();
                var user = await _loginProvider.GetUser(userId);

                var subscription = _contactRepository.GetContactDetails(user, contactId)
                                    .Log(_logger, "GetContactDetails")
                                    .Subscribe(contact => Clients.Caller.OnNext(contact),
                                               ex => Clients.Caller.OnError("Error receiving contacts"),
                                               () => Clients.Caller.OnCompleted());

                _subscription.Disposable = subscription;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Requesting ContactProfileStream in Hub.");
                Clients.Caller.OnError("Error receiving contact profile");
            }
        }

        public override Task OnDisconnected()
        {
            Debug.Print("ContactProfileHub.OnDisconnected()");
            _subscription.Dispose();
            return base.OnDisconnected();
        }
    }
}