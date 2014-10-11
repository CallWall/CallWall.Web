using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubContactSummary : IAccountContactSummary
    {
        private readonly List<string> _tags = new List<string>();

        public bool IsDeleted { get; set; }

        public string Provider { get; set; }

        public string AccountId { get; set; }

        public string ProviderId { get; set; }

        public string Title { get; set; }

        public string PrimaryAvatar { get; set; }

        public List<string> Tags { get { return _tags; } }
        IEnumerable<string> IAccountContactSummary.Tags { get { return Tags; } }        
    }
}