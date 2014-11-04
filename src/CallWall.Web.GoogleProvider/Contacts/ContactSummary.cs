using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Contacts
{
    internal class ContactSummary : IAccountContactSummary
    {
        private readonly string _title;
        private readonly IEnumerable<string> _tags;
        private readonly string _primaryAvatar;
        private readonly string _providerId;
        private readonly string _accountId;
        private readonly IEnumerable<ContactHandle> _handles;

        public ContactSummary(string providerId, string accountId, string title, string primaryAvatar, IEnumerable<string> tags, IEnumerable<ContactHandle> handles)
        {
            _providerId = providerId;
            _title = title;
            _primaryAvatar = primaryAvatar;
            _tags = tags;
            _handles = handles;
            _accountId = accountId;
        }

        public string Provider { get { return Constants.ProviderName; } }

        public string ProviderId { get { return _providerId; } }

        public string AccountId { get { return _accountId; } }

        /// <summary>
        /// The title description for the contact. Usually their First and Last name.
        /// </summary>
        public string Title { get { return _title; } }

        public IEnumerable<string> Tags { get { return _tags; } }

        public IEnumerable<ContactHandle> Handles { get { return _handles; } }

        public string PrimaryAvatar { get { return _primaryAvatar; } }

        public bool IsDeleted { get { return false; } }
    }
}
