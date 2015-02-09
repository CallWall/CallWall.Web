using System;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    //TODO: Why IUserContactRepository and (I)ContactRepository? Bit of feature envy, layers for layers sake here -LC 
    public interface IUserContactRepository
    {
        IObservable<Event<ContactAggregateUpdate>> GetContactSummariesFrom(User user, int? versionId);
        IObservable<int> ObserveContactUpdatesHeadVersion(User user);
        IObservable<IContactProfile> GetContactDetails(User user, string contactId);
        IObservable<IContactProfile> GetContactDetails(User user, string[] contactKeys);
    }
}