using System;
using CallWall.Web.EventStore.Accounts;

namespace CallWall.Web.EventStore.Users
{
    internal class UserRegisteredAccountEvent
    {
        public Guid UserId { get; set; }
        public AccountRecord Account { get; set; }
    }
}