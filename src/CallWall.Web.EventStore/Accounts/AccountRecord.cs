using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Accounts
{
    public class AccountRecord : IAccount
    {
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public ContactHandle[] Handles { get; set; }
        public SessionRecord CurrentSession { get; set; }

        public override string ToString()
        {
            return string.Format("Provider: {0}, AccountId: {1}, DisplayName: {2}, Handles: [{3}], CurrentSession: {4}",
                Provider,
                AccountId,
                DisplayName,
                string.Join(",", (Handles ?? Enumerable.Empty<ContactHandle>()).Select(ch => ch.Handle)),
                CurrentSession);
        }

        string IAccount.Provider { get { return Provider; } }
        string IAccount.AccountId { get { return AccountId; } }
        string IAccount.DisplayName { get { return DisplayName; } }
        IEnumerable<ContactHandle> IAccount.Handles { get { return Handles; } }
        ISession IAccount.CurrentSession { get { return CurrentSession; } }
    }
}