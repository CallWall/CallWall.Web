using System;

namespace CallWall.Web.EventStore.Contacts
{
    sealed class ContactSummaryRepository : IContactSummaryRepository
    {
        private readonly IUserContactRepository _userContactRepository;

        public ContactSummaryRepository(IUserContactRepository userContactRepository)
        {
            _userContactRepository = userContactRepository;
        }

        public IObservable<ContactAggregateUpdate> GetContactUpdates(User user, int fromEventId)
        {
            return _userContactRepository.GetContactSummariesFrom(user, fromEventId);
        }
    }
}
