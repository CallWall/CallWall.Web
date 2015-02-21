using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Providers.Contacts
{
    internal class ContactSummary : IAccountContactSummary
    {
        public ContactSummary(string providerId, string accountId, string title, string fullName, IAnniversary dateOfBirth, string primaryAvatar, 
            IEnumerable<string> tags, 
            IEnumerable<ContactHandle> handles,
            IEnumerable<IContactAssociation> organizations,
            IEnumerable<IContactAssociation> relationships
            )
        {
            ProviderId = providerId;
            AccountId = accountId;
            Title = title;
            FullName = fullName;
            DateOfBirth = dateOfBirth;
            
            AvatarUris = new[] { primaryAvatar };
            Tags = tags;
            Handles = handles;
            Organizations = organizations;
            Relationships = relationships;
        }

        public string Provider { get { return Constants.ProviderName; } }

        public string ProviderId { get; private set; }

        public string AccountId { get; private set; }

        /// <summary>
        /// The title description for the contact. Usually their First and Last name.
        /// </summary>
        public string Title { get; private set; }

        public string FullName { get; private set; }

        public IAnniversary DateOfBirth { get; private set; }
        
        public IEnumerable<string> AvatarUris { get; private set; }
        public IEnumerable<string> Tags { get; private set; }
        public IEnumerable<ContactHandle> Handles { get; private set; }
        public IEnumerable<IContactAssociation> Organizations { get; private set; }
        public IEnumerable<IContactAssociation> Relationships { get; private set; }

        public bool IsDeleted { get { return false; } }
    }
}
