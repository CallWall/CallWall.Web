using System.Collections.Generic;

namespace CallWall.Web.Domain
{
    public sealed class AccountContactSummaryComparer : IEqualityComparer<IAccountContactSummary>
    {
        private static readonly IEqualityComparer<IAccountContactSummary> _instance = new AccountContactSummaryComparer();
        public static IEqualityComparer<IAccountContactSummary> Instance { get { return _instance; } }


        public bool Equals(IAccountContactSummary x, IAccountContactSummary y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            if (x.IsDeleted || y.IsDeleted)
            {
                return x.IsDeleted && y.IsDeleted
                       && string.Equals(x.Provider, y.Provider)
                       && string.Equals(x.ProviderId, y.ProviderId)
                       && string.Equals(x.AccountId, y.AccountId);
            }

            return string.Equals(x.Provider, y.Provider)
                   && string.Equals(x.ProviderId, y.ProviderId)
                   && string.Equals(x.AccountId, y.AccountId)
                   && string.Equals(x.Title, y.Title)
                   && string.Equals(x.FullName, y.FullName)
                   && x.DateOfBirth.Equals(y.DateOfBirth)
                   && Equals(x.Tags, y.Tags)
                   && Equals(x.AvatarUris, y.AvatarUris)
                   && Equals(x.Handles, y.Handles)
                   && Equals(x.Organizations, y.Organizations)
                   && Equals(x.Relationships, y.Relationships);
        }

        public int GetHashCode(IAccountContactSummary obj)
        {
            unchecked
            {
                int hashCode = (obj.ProviderId != null ? obj.ProviderId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Provider != null ? obj.Provider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.AccountId != null ? obj.AccountId.GetHashCode() : 0);

                if (!obj.IsDeleted)
                {
                    hashCode = (hashCode * 397) ^ (obj.Title != null ? obj.Title.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.FullName != null ? obj.FullName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ obj.DateOfBirth.GetHashCode();
                    hashCode = (hashCode * 397) ^ (obj.Tags != null ? obj.Tags.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.AvatarUris != null ? obj.AvatarUris.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Handles != null ? obj.Handles.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Organizations != null ? obj.Organizations.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Relationships != null ? obj.Relationships.GetHashCode() : 0);
                }

                return hashCode;
            }
        }
    }
}