using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.InMemoryRepository
{
    sealed class ContactRepository : IContactRepository, IRunnable, IDisposable
    {
        private readonly SingleAssignmentDisposable _contactUpdateSubscription = new SingleAssignmentDisposable();
        private readonly Dictionary<Guid, ContactLookup> _userContactMap = new Dictionary<Guid, ContactLookup>();
        private readonly ContactFeedRepository _contactFeedRepository;

        public ContactRepository(ContactFeedRepository contactFeedRepository)
        {
            _contactFeedRepository = contactFeedRepository;
        }

        public IObservable<IContactProfile> GetContactDetails(User user, string contactId)
        {
            var contact = _userContactMap[user.Id].GetById(int.Parse(contactId));
            return Observable.Return(contact);
        }

        public IObservable<IContactProfile> LookupContactByHandles(User user, ContactHandle[] contactHandles)
        {
            var keys = contactHandles.SelectMany(ch => ch.NormalizedHandle())
                .ToArray();
            var matchedContacts = _userContactMap[user.Id].GetByContactKeys(keys);
            //HACK: This should be able to return multiple values. -LC
            return matchedContacts.ToObservable();
        }

        public async Task Run()
        {
            _contactUpdateSubscription.Disposable = _contactFeedRepository.GetAllUserContactUpdates()
                .SelectMany(userFeed =>
                    userFeed.ContactUpdates.Scan(new ContactLookup(userFeed.UserId), (acc, cur) => acc.Add(cur.Value)))
                .Subscribe(userContactLookup =>
                           {
                               _userContactMap[userContactLookup.UserId] = userContactLookup;
                           });
            //Fake asynchrony.
            await Task.FromResult(0);
        }

        public void Dispose()
        {
           _contactUpdateSubscription.Dispose();
        }
    }
}