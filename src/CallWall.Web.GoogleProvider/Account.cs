using CallWall.Web.Account;

namespace CallWall.Web.GoogleProvider
{
    internal sealed class Account : IAccount
    {
        private readonly string _username;
        private readonly string _displayName;
        private ISession _currentSession;

        public Account(string username, string displayName)
        {
            _username = username;
            _displayName = displayName;
        }

        public string Provider { get { return "Google"; } }

        public string Username
        {
            get { return _username; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        //TODO: Implement setting the session for a Google Provider Account; -LC
        public ISession CurrentSession { get { return _currentSession; } }
    }
}