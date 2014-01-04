using System.Collections.Generic;

namespace CallWall.Web.GoogleProvider.Contacts
{
    internal class ContactSummary : IContactSummary
    {
        private readonly string _title;
        private readonly IEnumerable<string> _tags;
        private readonly string _primaryAvatar;
        private readonly string _id;

        public ContactSummary(string id, string title, string primaryAvatar, IEnumerable<string> tags)
        {
            _id = string.Format("Google-{0}", id);
            _title = title;
            _primaryAvatar = primaryAvatar;
            _tags = tags;
        }
        
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// The title description for the contact. Usually their First and Last name.
        /// </summary>
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
