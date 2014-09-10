using System;
using CallWall.Web.EventStore.Accounts;

namespace CallWall.Web.EventStore.Contacts
{
    public class RefreshContactsCommand : IAccountData
    {
        public DateTimeOffset TimeStamp { get; set; }
        public string RefreshTrigger { get; set; }

        public Guid UserId { get; set; }

        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public SessionRecord CurrentSession { get; set; }
        ISession IAccountData.CurrentSession { get { return CurrentSession; }}
    }
}