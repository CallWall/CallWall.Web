using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;

namespace CallWall.Web.LinkedInProvider.Contacts
{
    public class ContactSummary : IAccountContactSummary
    {
        private readonly string _title;
        private readonly IEnumerable<string> _tags;
        private readonly IEnumerable<IContactAssociation> _organizations;
        private readonly string _primaryAvatar;
        private readonly string _providerId;
        private readonly string _accountId;

        public ContactSummary(string accountId, string id, string firstname, string lastname, string primaryAvatar, IEnumerable<string> tags, IEnumerable<IContactAssociation> organizations)
        {
            _accountId = accountId;
            _providerId = id;
            _title = string.Format("{0} {1}", firstname, lastname);
            _primaryAvatar = primaryAvatar;
            _tags = tags;
            _organizations = organizations;
        }

        public bool IsDeleted { get; private set; }
        public string Provider { get { return "LinkedIn"; } }

        public string AccountId { get { return _accountId; } }

        public string ProviderId { get { return _providerId; } }

        public string Title { get { return _title; } }
        public string FullName { get { return _title; } }

        public IEnumerable<string> Tags { get { return _tags; } }

        public IEnumerable<string> AvatarUris
        {
            get { return Enumerable.Repeat(_primaryAvatar, 1); }
        }

        public IEnumerable<IContactAssociation> Organizations { get { return _organizations; } }



        IAnniversary IAccountContactSummary.DateOfBirth
        {
            get { return null; }
        }
        IEnumerable<ContactHandle> IAccountContactSummary.Handles
        {
            get { return Enumerable.Empty<ContactHandle>(); }
        }
        IEnumerable<IContactAssociation> IAccountContactSummary.Relationships
        {
            get { return Enumerable.Empty<IContactAssociation>(); }
        }
    }
}