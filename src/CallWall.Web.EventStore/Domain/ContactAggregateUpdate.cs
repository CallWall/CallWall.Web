namespace CallWall.Web.EventStore.Domain
{
    public class ContactAggregateUpdate
    {
        public int Id { get; set; }
        public int Version { get; set; }

        public bool IsDeleted { get; set; }

        public string NewTitle { get; set; }

        public string[] AddedTags { get; set; }
        public string[] RemovedTags { get; set; }

        public string[] AddedAvatars { get; set; }
        public string[] RemovedAvatars { get; set; }

        public ContactProviderSummary[] AddedProviders { get; set; }
        public ContactProviderSummary[] RemovedProviders { get; set; }
    }
}