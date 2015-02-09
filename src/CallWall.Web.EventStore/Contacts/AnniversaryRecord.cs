using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class AnniversaryRecord : IAnniversary
    {
        public int? Year { get; set; }
        public int Month { get; set; }
        public int DayOfMonth { get; set; }
    }
}