namespace CallWall.Web.Domain
{
    //TODO: To send to the UI, only need Id, Version, IsDeleted, NewTitle & Add/Removed Avatars -LC
    //TODO: Add EventId? This way the Hub can filter where NewTitle==null && Add/Removed Avatars==null
    //TODO: May need to send empty update with eventId if the last batch was all filtered out (to prevent requesting data we have consumed but didn't want) -LC
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