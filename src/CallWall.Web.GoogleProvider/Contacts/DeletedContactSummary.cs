using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Contacts
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

        string IAccountContactSummary.PrimaryAvatar
        {
            get { throw new System.NotSupportedException(); }
        }

        IEnumerable<string> IAccountContactSummary.Tags
        {
            get { throw new System.NotSupportedException(); }
        }

        public IEnumerable<ContactHandle> Handles { get { return Enumerable.Empty<ContactHandle>(); } }
    }
}