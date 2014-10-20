namespace CallWall.Web.Domain
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

        public ContactHandle[] AddedHandles { get; set; }
        public ContactHandle[] RemovedHandles { get; set; }

        public override string ToString()
        {
            if (IsDeleted)
            {
                return string.Format("ContactAggregateUpdate{{ Id:{0}, Version:{1}, IsDeleted:true}}", Id, Version);
            }
            return string.Format("ContactAggregateUpdate{{ Id:{0}, Version:{1}, NewTitle:{2}}}", Id, Version, NewTitle);
        }
    }
}