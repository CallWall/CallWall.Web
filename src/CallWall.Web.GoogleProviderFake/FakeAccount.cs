﻿using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProviderFake
{
    sealed class FakeAccount : IAccount
    {
        public string Provider { get { return "GoogleFake"; } }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<ContactHandle> Handles { get; set; }
        public ISession CurrentSession { get; set; }
    }
}