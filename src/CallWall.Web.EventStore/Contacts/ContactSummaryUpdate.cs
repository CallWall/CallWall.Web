using System.Collections.Generic;

namespace CallWall.Web.EventStore.Contacts
{
    public class ContactSummaryUpdate : IContactSummaryUpdate
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
        public string[] Tags { get; set; }
        public string[] Avatars { get; set; }
        public IContactProviderSummary[] Providers { get; set; }

        IEnumerable<string> IContactSummaryUpdate.Tags { get { return Tags; } }
        IEnumerable<string> IContactSummaryUpdate.Avatars { get { return Avatars; } }
        IEnumerable<IContactProviderSummary> IContactSummaryUpdate.Providers { get { return Providers; } }
    }
}
