using System;
using System.Collections.Generic;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Accounts;

namespace CallWall.Web.EventStore.Contacts
{
    public class RefreshContactsCommand : IAccount
    {
        public DateTimeOffset TimeStamp { get; set; }
        public string RefreshTrigger { get; set; }

        public Guid UserId { get; set; }

        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public ContactHandle[] Handles { get; set; }
        public SessionRecord CurrentSession { get; set; }

        IEnumerable<ContactHandle> IAccount.Handles { get { return Handles; } }
        ISession IAccount.CurrentSession { get { return CurrentSession; }}
    }
}