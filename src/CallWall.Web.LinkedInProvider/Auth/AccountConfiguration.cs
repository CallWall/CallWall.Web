﻿using System;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.LinkedInProvider.Auth
{
    public sealed class AccountConfiguration : IAccountConfiguration
    {
        public static readonly IAccountConfiguration Instance = new AccountConfiguration();

        private AccountConfiguration()
        {}

        public string Name { get { return "LinkedIn"; } }
        public Uri Image { get { return new Uri("/Content/LinkedIn/Images/LinkedInLogo.png", UriKind.Relative); } }
        public IEnumerable<IResourceScope> Resources { get { return ResourceScope.AvailableResourceScopes; } }
    }
}