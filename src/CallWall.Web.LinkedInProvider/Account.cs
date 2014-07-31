using CallWall.Web.Account;
namespace CallWall.Web.LinkedInProvider
{
    public sealed class Account : IAccount
    {
        private readonly string _userName;
        private readonly string _displayName;
        private string _provider;
        private ISession _currentSession;

        public Account(string userName, string displayName)
        {
            _userName = userName;
            _displayName = displayName;
        }

        public string Provider { get { return "LinkedIn"; } }

        public string Username { get { return _userName; } }
        public string DisplayName { get { return _displayName; } }
        //TODO: Implement setting the session for a LinkedIn Provider Account; -LC
        public ISession CurrentSession { get { return _currentSession; } }
    }
}