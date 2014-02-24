using System;
using System.Collections.Generic;
using CallWall.Web.Account;

namespace CallWall.Web.GoogleProvider.Auth
{
    internal sealed class AccountConfiguration : IAccountConfiguration
    {
        public static readonly IAccountConfiguration Instance = new AccountConfiguration();

        private AccountConfiguration()
        {}

        public string Name { get { return "Google"; } }
        public Uri Image { get { return new Uri("/Content/Google/Images/GoogleIcon.svg", UriKind.Relative); } }
        public IEnumerable<IResourceScope> Resources { get { return ResourceScope.AvailableResourceScopes; } }
    }
}