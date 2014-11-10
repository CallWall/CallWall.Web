using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public class ContactAssociationRecord : IContactAssociation
    {
        public string Name { get; set; }
        public string Association { get; set; }
    }
}