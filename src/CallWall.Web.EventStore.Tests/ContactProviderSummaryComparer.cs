using System;
using System.Collections;
using System.Collections.Generic;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Tests
{
    sealed class ContactProviderSummaryComparer : IComparer, IComparer<IContactProviderSummary>
    {
        public static readonly ContactProviderSummaryComparer Instance = new ContactProviderSummaryComparer();
        public int Compare(object x, object y)
        {
            var lhs = x as IContactProviderSummary;
            var rhs = y as IContactProviderSummary;
            return Compare(lhs, rhs);
        }

        public int Compare(IContactProviderSummary x, IContactProviderSummary y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var providerSort = String.CompareOrdinal(x.ProviderName, y.ProviderName);
            if (providerSort != 0) return providerSort;

            var accountSort = String.CompareOrdinal(x.AccountId, y.AccountId);
            if (accountSort != 0) return accountSort;

            return String.CompareOrdinal(x.ContactId, y.ContactId);
        }
    }
}