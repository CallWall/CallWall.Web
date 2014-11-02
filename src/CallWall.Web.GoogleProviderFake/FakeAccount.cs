using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProviderFake
{
    sealed class FakeAccount : IAccount
    {
        public string Provider { get { return Constants.ProviderName; } }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<ContactHandle> Handles { get { return Enumerable.Empty<ContactHandle>(); } }
        public ISession CurrentSession { get; set; }
    }
}