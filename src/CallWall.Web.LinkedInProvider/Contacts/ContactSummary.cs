using System.Collections.Generic;
using CallWall.Web.Contracts;

namespace CallWall.Web.LinkedInProvider.Contacts
{
    public class ContactSummary : IContactSummary
    {
        //TODO copy and paste job here - see google

        private readonly string _title;
        private readonly IEnumerable<string> _tags;
        private readonly string _primaryAvatar;
        private readonly string _providerId;
        private readonly string _accountId;

        public ContactSummary(string accountId, string id, string firstname, string lastname, string primaryAvatar, IEnumerable<string> tags)
        {
            _accountId = accountId;
            _providerId = id;
            _title = string.Format("{0} {1}", firstname, lastname);
            _primaryAvatar = primaryAvatar;
            _tags = tags;
        }

        public string Provider { get { return "LinkedIn"; } }

        public string AccountId
        {
            get { return _accountId; }
        }

        public string ProviderId
        {
            get { return _providerId; }
        }

        public string Title
        {
            get { return _title; }
        }

        public IEnumerable<string> Tags
        {
            get { return _tags; }
        }

        public string PrimaryAvatar
        {
            get { return _primaryAvatar; }
        }
    }
}