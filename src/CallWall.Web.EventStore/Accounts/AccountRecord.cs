using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;
using CallWall.Web.EventStore.Contacts;

namespace CallWall.Web.EventStore.Accounts
{
    public class AccountRecord : IAccount
    {
        public AccountRecord()
        {
        }

        public AccountRecord(IAccount source)
        {
            Provider = source.Provider;
            AccountId = source.AccountId;
            DisplayName = source.DisplayName;
            Handles = source.Handles.Select(h => new ContactHandleRecord(h)).ToArray();
            CurrentSession = new SessionRecord(source.CurrentSession);
        }

        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public ContactHandleRecord[] Handles { get; set; }
        public SessionRecord CurrentSession { get; set; }

        public override string ToString()
        {
            return string.Format("Provider: {0}, AccountId: {1}, DisplayName: {2}, Handles: [{3}], CurrentSession: {4}",
                Provider,
                AccountId,
                DisplayName,
                string.Join(",", (Handles ?? Enumerable.Empty<ContactHandleRecord>()).Select(ch => ch.Handle)),
                CurrentSession);
        }

        string IAccount.Provider { get { return Provider; } }
        string IAccount.AccountId { get { return AccountId; } }
        string IAccount.DisplayName { get { return DisplayName; } }
        IEnumerable<ContactHandle> IAccount.Handles { get { return Handles.Select(h => h.ToContactHandle()); } }
        ISession IAccount.CurrentSession { get { return CurrentSession; } }
    }
}