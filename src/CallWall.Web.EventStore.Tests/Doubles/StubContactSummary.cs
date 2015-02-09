using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Tests.Doubles
{
    public class StubContactSummary : IAccountContactSummary
    {
        private readonly List<string> _avatarUris = new List<string>();
        private readonly List<string> _tags = new List<string>();
        private readonly List<ContactHandle> _handles = new List<ContactHandle>();
        private readonly List<IContactAssociation> _organizations = new List<IContactAssociation>();
        private readonly List<IContactAssociation> _relationships = new List<IContactAssociation>();
        

        public bool IsDeleted { get; set; }
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string ProviderId { get; set; }
        public string Title { get; set; }
        public IAnniversary DateOfBirth { get; set; }
        public string FullName { get; private set; }

        public List<string> AvatarUris { get { return _avatarUris; } }
        public List<string> Tags { get { return _tags; }}
        public List<ContactHandle> Handles { get { return _handles; } }
        public List<IContactAssociation> Organizations { get { return _organizations; } }
        public List<IContactAssociation> Relationships { get { return _relationships; } }


        IAnniversary IAccountContactSummary.DateOfBirth { get { return DateOfBirth; } }
        IEnumerable<string> IAccountContactSummary.AvatarUris { get { return AvatarUris; } }
        IEnumerable<string> IAccountContactSummary.Tags { get { return Tags; } }
        IEnumerable<ContactHandle> IAccountContactSummary.Handles { get { return Handles; } }
        IEnumerable<IContactAssociation> IAccountContactSummary.Organizations { get { return Organizations; } }
        IEnumerable<IContactAssociation> IAccountContactSummary.Relationships { get { return Relationships; } }

        public StubContactSummary Clone()
        {
            var clone = new StubContactSummary
            {
                IsDeleted = this.IsDeleted,
                Provider = this.Provider,
                AccountId = this.AccountId,
                ProviderId = this.ProviderId,
                Title = this.Title,
                DateOfBirth = this.DateOfBirth,
                FullName = this.FullName,
            };

            clone.AvatarUris.AddRange(this.AvatarUris);
            clone.Tags.AddRange(this.Tags);
            clone.Handles.AddRange(this.Handles);
            clone.Organizations.AddRange(this.Organizations);
            clone.Relationships.AddRange(this.Relationships);

            return clone;
        }
    }
}