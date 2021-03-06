using CallWall.Web.Domain;

namespace CallWall.Web.LinkedInProvider.Contacts
{
    sealed class ContactAssociation : IContactAssociation
    {
        private readonly string _name;
        private readonly string _association;

        public ContactAssociation(string name, string association)
        {
            _name = name;
            _association = association;
        }

        public string Name { get { return _name; } }

        public string Association { get { return _association; } }
    }
}