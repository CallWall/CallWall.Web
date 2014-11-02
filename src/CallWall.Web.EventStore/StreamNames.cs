namespace CallWall.Web.EventStore
{
    internal static class StreamNames
    {
        public static readonly string ContactSummaryRefreshRequest = "ContactSummaryRefreshRequest";
        public static readonly string ContactSummaryProviderRefreshRequest = "ContactSummaryProviderRefreshRequest";

        public static string ContactSummaryUpdates(int userId)
        {
            return string.Format("ContactSummaryUpdates-{0}", userId);
        }

        public static string ContactSummaryRecieved(int userId, string provider)
        {
            return string.Format("ContactSummaryRecieved-{0}-{1}", userId, provider);
        }
    }
}