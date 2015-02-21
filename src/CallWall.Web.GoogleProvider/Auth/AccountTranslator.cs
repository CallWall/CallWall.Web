using System.Xml.Linq;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Auth
{
    class AccountTranslator : IAccountTranslator
    {
        private readonly IAccountFactory _accountFactory;

        public AccountTranslator(IAccountFactory accountFactory)
        {
            _accountFactory = accountFactory;
        }

        public IAccount TranslateToAccount(string response, ISession session)
        {
            var xDoc = XDocument.Parse(response);
            if (xDoc.Root == null)
                return null;
            var root = xDoc.Root;
            var idElement = root.Element("x", "id");
            if (idElement == null)
                return null;
            var xAuthor = xDoc.Root.Element("x", "author");
            if (xAuthor == null)
                return null;

            var xName = xAuthor.Element("x", "name");
            if (xName == null)
                return null;
            var xEmail = xAuthor.Element("x", "email");
            if (xEmail == null)
                return null;

            var id = idElement.Value;
            var name = xName.Value;
            var email = xEmail.Value;
            var contactHandles = new ContactHandle[] { new ContactEmailAddress(email, "main") };

            return _accountFactory.Create(id, Constants.ProviderName, name, session, contactHandles);
        }
        
    }
}