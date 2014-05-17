using System.Collections.Generic;

namespace CallWall.Web.EventStore.Events
{
    public class ContactSummaryUpdate : IContactSummaryUpdate
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<string> Avatars { get; set; }
        public IEnumerable<IContactProviderSummary> Providers { get; set; }
    }
}