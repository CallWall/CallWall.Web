using System;
using CallWall.Web.EventStore.Accounts;

namespace CallWall.Web.EventStore.Users
{
    internal class UserCreatedEvent
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public AccountRecord Account { get; set; }
    }
}