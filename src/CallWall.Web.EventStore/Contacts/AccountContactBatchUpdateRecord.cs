using System;

namespace CallWall.Web.EventStore.Contacts
{
    public class AccountContactBatchUpdateRecord
    {
        public Guid UserId { get; set; }
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public AccountContactRecord[] Contacts { get; set; }
    }
}