using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class AccountContactRecord : IAccountContactSummary
    {
        public bool IsDeleted { get; set; }
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string ProviderId { get; set; }
        public string Title { get; set; }
        public AnniversaryRecord DateOfBirth { get; set; }
        public string FullName { get; private set; }
                      
        public string[] AvatarUris { get; set; }
        public string[] Tags { get; set; }
        public ContactHandle[] Handles { get; set; }
        public ContactAssociationRecord[] Organizations { get; set; }
        public ContactAssociationRecord[] Relationships { get; set; }


        IAnniversary IAccountContactSummary.DateOfBirth { get { return DateOfBirth; }}
        IEnumerable<string> IAccountContactSummary.AvatarUris { get { return AvatarUris; } }
        IEnumerable<string> IAccountContactSummary.Tags { get { return Tags; } }
        IEnumerable<ContactHandle> IAccountContactSummary.Handles { get { return Handles; } }
        IEnumerable<IContactAssociation> IAccountContactSummary.Organizations { get { return Organizations; }}
        IEnumerable<IContactAssociation> IAccountContactSummary.Relationships { get { return Relationships; } }
    }
}