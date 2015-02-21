using System;

namespace CallWall.Web.EventStore.Contacts
{
    public static class ContactStreamNames
    {
        public static string AccountContacts(string provider, string accountId)
        {
            //return "AccountContactsBlah";
            return string.Format("AccountContacts-{0}-{1}", provider, accountId);
        }

        public static string UserContacts(Guid userId)
        {
            return string.Format("UserContacts-{0}", userId);
        }

        public static string AccountRefreshRequests()
        {
            return "AccountRefreshRequests";
        }
    }
}