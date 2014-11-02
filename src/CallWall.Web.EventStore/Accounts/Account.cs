using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Accounts
{
    public class Account : IAccount
    {
        public string Provider { get; set; }
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public ISession CurrentSession { get; set; }
        
        #region Equality operators

        private bool Equals(IAccount other)
        {
            return string.Equals(Provider, other.Provider)
                   && string.Equals(AccountId, other.AccountId)
                   && string.Equals(DisplayName, other.DisplayName)
                   && Equals(CurrentSession, other.CurrentSession);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as IAccount;
            if (other == null) return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Provider != null ? Provider.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (AccountId != null ? AccountId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (CurrentSession != null ? CurrentSession.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Account left, IAccount right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Account left, IAccount right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Provider: {0}, AccountId: {1}, DisplayName: {2}, CurrentSession: {3}", Provider, AccountId, DisplayName, CurrentSession);
        }
    }
}