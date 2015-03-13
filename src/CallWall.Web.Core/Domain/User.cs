using System;
using System.Collections.Generic;
using System.Linq;

namespace CallWall.Web.Domain
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

        public User AddAccount(IAccount account)
        {
            return new User(_id, _displayName, _accounts.Concat(new[] { account }));
        }

        public override string ToString()
        {
            return string.Format("Id:'{0}', DisplayName:'{1}', Accounts:'{2}'", Id, DisplayName, string.Join(",", Accounts.Select(a=>a.AccountId)));
        }
    }
}