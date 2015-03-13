using System;
using System.Collections.Generic;
using System.Linq;
using CallWall.Web.Domain;

namespace CallWall.Web.Models
{
    public class OAuthAccountListItem
    {
        public OAuthAccountListItem(IAccount account, IProviderConfiguration providerConfiguration)
        {
            AccountId = account.AccountId;
            Name = providerConfiguration.Name;
            Image = providerConfiguration.Image;
            if (account.Handles.Any())
            {
                AccountHandle = account.Handles.Single().Handle;
            }
            else
            {
                AccountHandle = account.DisplayName;
            }

            var authorizedResources = account.CurrentSession.AuthorizedResources.ToSet();
            ResourceSelections = providerConfiguration.Resources
                .Select(r => new ResourceScopeSelection(r, authorizedResources.Contains(r.Resource)))
                .ToArray();
        }

        public string AccountId { get; private set; }
        public string AccountHandle { get; private set; }
        public string Name { get; private set; }
        public Uri Image { get; private set; }
        public IEnumerable<ResourceScopeSelection> ResourceSelections{ get; private set; }
    }
}