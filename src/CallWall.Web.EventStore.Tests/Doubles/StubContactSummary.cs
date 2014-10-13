using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubContactSummary : IAccountContactSummary
    {
        private readonly List<string> _tags = new List<string>();
        private readonly List<ContactHandle> _handles = new List<ContactHandle>();

        public bool IsDeleted { get; set; }

        public string Provider { get; set; }

        public string AccountId { get; set; }

        public string ProviderId { get; set; }

        public string Title { get; set; }

        public string PrimaryAvatar { get; set; }

        public List<string> Tags { get { return _tags; } }
        IEnumerable<string> IAccountContactSummary.Tags { get { return Tags; } }

        public List<ContactHandle> Handles { get { return _handles; } }
        IEnumerable<ContactHandle> IAccountContactSummary.Handles { get { return Handles; } }

        public StubContactSummary Clone()
        {
            var clone = new StubContactSummary
            {
                IsDeleted = this.IsDeleted,
                Provider = this.Provider,
                AccountId = this.AccountId,
                ProviderId = this.ProviderId,
                Title = this.Title,
                PrimaryAvatar = this.PrimaryAvatar,

            };

            clone.Tags.AddRange(this.Tags);
            clone.Handles.AddRange(this.Handles);

            return clone;
        }
    }
}