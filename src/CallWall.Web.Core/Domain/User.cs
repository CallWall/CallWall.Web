using System;
using System.Collections.Generic;
using System.Linq;

namespace CallWall.Web
{
    public class User
    {
        private readonly bool _isAuthenticated;
        private readonly Guid _id;
        private readonly string _displayName;
        private readonly IEnumerable<IAccount> _accounts;
        public static readonly User AnonUser = new User();

        private User()
        {
            _id = Guid.Empty;
            _displayName = null;
            _accounts = Enumerable.Empty<IAccount>();
            _isAuthenticated = false;
        }

        public User(Guid id, string displayName, IEnumerable<IAccount> accounts)
        {
            _id = id;
            _displayName = displayName;
            _accounts = accounts;
            _isAuthenticated = true;
        }

        public bool IsAuthenticated { get { return _isAuthenticated; } }
        
        public Guid Id { get { return _id; } }

        public string DisplayName { get { return _displayName; } }

        public IEnumerable<IAccount> Accounts { get { return _accounts; } }
    }
}