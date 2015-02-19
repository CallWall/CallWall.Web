namespace CallWall.Web.Domain
{
    public class ContactAssociation : IContactAssociation
    {
        public ContactAssociation()
        {
        }

        public ContactAssociation(string association,string name)
        {            
            Association = association;
            Name = name;
        }

        public string Name { get; set; }
        public string Association { get; set; }
    }
}