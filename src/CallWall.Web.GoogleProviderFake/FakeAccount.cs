using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProviderFake
{
    sealed class FakeAccount : IAccount
    {
        public FakeAccount(string accountId, string displayName, string handle)
        {
            AccountId = accountId;
            DisplayName = displayName;
            Handles = new[] { new ContactEmailAddress(handle, "Home") };
        }
        public string Provider { get { return Constants.ProviderName; } }
        public string AccountId { get; private set; }
        public string DisplayName { get; private set; }
        public IEnumerable<ContactHandle> Handles { get; private set; }
        public ISession CurrentSession { get; set; }
    }
}