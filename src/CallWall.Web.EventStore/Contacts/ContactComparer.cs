using System.Collections.Generic;

namespace CallWall.Web.EventStore.Contacts
{
    public sealed class ContactComparer : IEqualityComparer<IAccountContactSummary>
    {
        private static readonly IEqualityComparer<IAccountContactSummary> _instance = new ContactComparer();
        public static IEqualityComparer<IAccountContactSummary> Instance { get { return _instance; } }


        public bool Equals(IAccountContactSummary x, IAccountContactSummary y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Provider, y.Provider) && string.Equals(x.AccountId, y.AccountId) && string.Equals(x.ProviderId, y.ProviderId) && string.Equals(x.Title, y.Title) && string.Equals(x.PrimaryAvatar, y.PrimaryAvatar);
        }

        public int GetHashCode(IAccountContactSummary obj)
        {
            unchecked
            {
                int hashCode = (obj.Provider != null ? obj.Provider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.AccountId != null ? obj.AccountId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.ProviderId != null ? obj.ProviderId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Title != null ? obj.Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.PrimaryAvatar != null ? obj.PrimaryAvatar.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}