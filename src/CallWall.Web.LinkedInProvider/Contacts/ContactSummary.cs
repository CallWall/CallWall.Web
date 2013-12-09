using System.Collections.Generic;

namespace CallWall.Web.LinkedInProvider.Contacts
{
    public class ContactSummary : IContactSummary
    {
        //TODO copy and paste job here - see google

        private readonly string _title;
        private readonly IEnumerable<string> _tags;
        private readonly string _primaryAvatar;

        public ContactSummary(string firstname, string lastname, string primaryAvatar, IEnumerable<string> tags)
        {
            _title = string.Format("{0} {1}", firstname, lastname);
            _primaryAvatar = primaryAvatar;
            _tags = tags;
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