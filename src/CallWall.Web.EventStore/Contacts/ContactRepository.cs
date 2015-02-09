using System;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    //TODO: Seems a bit envious of IUserContactRepository eh -LC
    sealed class ContactRepository : IContactRepository
    {
        private readonly IUserContactRepository _userContactRepository;

        public ContactRepository(IUserContactRepository userContactRepository)
        {
            _userContactRepository = userContactRepository;
        }

        public IObservable<Event<ContactAggregateUpdate>> GetContactUpdates(User user, int fromEventId)
        {
            return _userContactRepository.GetContactSummariesFrom(user, fromEventId);
        }

        public IObservable<int> ObserveContactUpdatesHeadVersion(User user)
        {
            return _userContactRepository.ObserveContactUpdatesHeadVersion(user);
        }

        public IObservable<IContactProfile> GetContactDetails(User user, string contactId)
        {
            return _userContactRepository.GetContactDetails(user, contactId);
        }
    }
}
