using System;
using CallWall.Web.Domain;

namespace CallWall.Web.InMemoryRepository
{
    public class AccountContactRefreshRequest
    {
        private readonly Guid _userId;
        private readonly IAccount _account;
        private readonly ContactRefreshTriggers _triggeredBy;

        public AccountContactRefreshRequest(Guid userId, IAccount account, ContactRefreshTriggers triggeredBy)
        {
            _userId = userId;
            _account = account;
            _triggeredBy = triggeredBy;
        }

        public Guid UserId { get { return _userId; } }

        public IAccount Account { get { return _account; } }

        public ContactRefreshTriggers TriggeredBy { get { return _triggeredBy; } }
    }
}