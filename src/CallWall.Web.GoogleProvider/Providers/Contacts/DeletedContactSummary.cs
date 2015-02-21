using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Providers.Contacts
{
    public sealed class DeletedContactSummary : IAccountContactSummary
    {
        private readonly string _providerId;
        private readonly string _accountId;

        public DeletedContactSummary(string providerId, string accountId)
        {
            _providerId = providerId;
            _accountId = accountId;
        }

        public bool IsDeleted { get { return true; } }

        public string Provider { get { return Constants.ProviderName; } }

        public string ProviderId { get { return _providerId; } }

        public string AccountId { get { return _accountId; } }

        string IAccountContactSummary.Title
        {
            get { throw new System.NotSupportedException(); }
        }

        IEnumerable<string> IAccountContactSummary.Tags
        {
            get { throw new System.NotSupportedException(); }
        }

        IAnniversary IAccountContactSummary.DateOfBirth
        {
            get { throw new System.NotImplementedException(); }
        }

        string IAccountContactSummary.FullName
        {
            get { throw new System.NotImplementedException(); }
        }

        IEnumerable<string> IAccountContactSummary.AvatarUris
        {
            get { throw new System.NotImplementedException(); }
        }

        IEnumerable<ContactHandle> IAccountContactSummary.Handles
        {
            get { throw new System.NotImplementedException(); }
        }

        IEnumerable<IContactAssociation> IAccountContactSummary.Organizations
        {
            get { throw new System.NotImplementedException(); }
        }

        IEnumerable<IContactAssociation> IAccountContactSummary.Relationships
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}