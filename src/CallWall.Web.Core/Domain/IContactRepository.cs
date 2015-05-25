using System;

namespace CallWall.Web.Domain
{
    public interface IContactRepository
    {
        IObservable<IContactProfile> GetContactDetails(User user, string contactId);

        /// <summary>
        /// Looks for matching contacts from the provided handles. 0, 1 or many results may be returned.
        /// </summary>
        /// <param name="user">The user performing the lookup</param>
        /// <param name="contactHandles">The handles to use to perform a match</param>
        /// <returns>An observable sequence with 0, 1 or many contact matches.</returns>
        IObservable<IContactProfile> LookupContactByHandles(User user, ContactHandle[] contactHandles);
    }
}