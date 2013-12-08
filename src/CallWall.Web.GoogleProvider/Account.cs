namespace CallWall.Web.GoogleProvider
{
    internal sealed class Account : IAccount
    {
        private readonly string _username;
        private readonly string _displayName;

        public Account(string username, string displayName)
        {
            _username = username;
            _displayName = displayName;
        }

        public string Username
        {
            get { return _username; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }
    }
}