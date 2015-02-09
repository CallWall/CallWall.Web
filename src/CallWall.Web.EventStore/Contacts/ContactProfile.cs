using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    sealed class ContactProfile : IContactProfile
    {
        private readonly List<string> _avatarUris = new List<string>();
        private readonly List<string> _tags = new List<string>();
        private readonly List<IContactAssociation> _organizations = new List<IContactAssociation>();
        private readonly List<IContactAssociation> _relationships = new List<IContactAssociation>();
        private readonly List<ContactHandle> _handles = new List<ContactHandle>();

        public ContactProfile(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }

        public string Title { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public List<string> AvatarUris { get { return _avatarUris; } }
        public List<string> Tags { get { return _tags; } }
        public List<IContactAssociation> Organizations{get { return _organizations; }}
        public List<IContactAssociation> Relationships{get { return _relationships; }}
        public List<ContactHandle> Handles{get { return _handles; }}

        IEnumerable<string> IContactProfile.AvatarUris { get { return AvatarUris; } }
        IEnumerable<string> IContactProfile.Tags { get { return Tags; } }
        IEnumerable<IContactAssociation> IContactProfile.Organizations { get { return Organizations; } }
        IEnumerable<IContactAssociation> IContactProfile.Relationships { get { return Relationships; } }
        IEnumerable<ContactHandle> IContactProfile.Handles { get { return Handles; } }
        
    }
}