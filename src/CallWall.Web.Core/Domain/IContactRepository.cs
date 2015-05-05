using System;

namespace CallWall.Web.Domain
{
    public interface IContactRepository
    {
        IObservable<IContactProfile> GetContactDetails(User user, string contactId);

        /// <summary>
        /// Looks for matching contacts from the provided keys. 0, 1 or many results may be returned.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="contactKeys"></param>
        /// <returns></returns>
        IObservable<IContactProfile> LookupContactByKey(User user, string[] contactKeys);
    }
}