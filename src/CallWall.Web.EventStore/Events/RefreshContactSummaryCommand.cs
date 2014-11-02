namespace CallWall.Web.EventStore.Events
{
    public class RefreshContactSummaryCommand
    {
        private readonly int _userId;
        private readonly int _fromEventId;

        public RefreshContactSummaryCommand(int userId, int fromEventId)
        {
            _userId = userId;
            _fromEventId = fromEventId;
        }

        public int UserId
        {
            get { return _userId; }
        }

        public int FromEventId
        {
            get { return _fromEventId; }
        }
    }
}