using System;
using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.InMemoryRepository
{
    public class ContactRepository : IContactRepository, IRunnable, IDisposable
    {
        private readonly IContactFeedRepository _contactFeedRepository;

        public ContactRepository(IContactFeedRepository contactFeedRepository)
        {
            _contactFeedRepository = contactFeedRepository;
        }

        public IObservable<IContactProfile> GetContactDetails(User user, string contactId)
        {
            throw new NotImplementedException();
        }

        public IObservable<IContactProfile> LookupContactByKey(User user, string[] contactKeys)
        {
            throw new NotImplementedException();
        }

        public Task Run()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}