using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class AccountContactRecord : IAccountContactSummary
    {
        public bool IsDeleted { get; set; }
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string ProviderId { get; set; }
        public string Title { get; set; }
        public string PrimaryAvatar { get; set; }
        public string[] Tags { get; set; }
        public ContactHandle[] Handles { get; set; }

        IEnumerable<string> IAccountContactSummary.Tags { get { return Tags; } }
        IEnumerable<ContactHandle> IAccountContactSummary.Handles { get { return Handles; } }
    }
}