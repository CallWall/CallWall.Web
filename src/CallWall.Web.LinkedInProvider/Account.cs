using CallWall.Web.Account;
namespace CallWall.Web.LinkedInProvider
{
    public sealed class Account : IAccount
    {
        private readonly string _userName;
        private readonly string _displayName;

        public Account(string userName, string displayName)
        {
            _userName = userName;
            _displayName = displayName;
        }

        public string Username { get { return _userName; } }
        public string DisplayName { get { return _displayName; } }
    }
}