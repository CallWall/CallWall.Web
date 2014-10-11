using System;
using System.Collections.Generic;

namespace CallWall.Web
{
    public interface ISession
    {
        string AccessToken { get; }
        string RefreshToken { get; }
        DateTimeOffset Expires { get; }
        bool HasExpired();
        ISet<string> AuthorizedResources { get; }
    }

    public class Session : ISession
    {
        private readonly string _accessToken;
        private readonly string _refreshToken;
        private readonly DateTimeOffset _expires;
        private readonly ISet<string> _authorizedResources;

        public Session(string accessToken, string refreshToken, DateTimeOffset expires, IEnumerable<string> authorizedResources)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _expires = expires;
            _authorizedResources = new HashSet<string>(authorizedResources);
        }

        public string AccessToken { get { return _accessToken; } }

        public string RefreshToken { get { return _refreshToken; } }

        public DateTimeOffset Expires { get { return _expires; } }

        public bool HasExpired()
        {
            return DateTimeOffset.Now > _expires;
        }

        public ISet<string> AuthorizedResources { get { return _authorizedResources; } }

        public override string ToString()
        {
            return string.Format("Session {{ AccessToken : '{0}', RefreshToken : '{1}', Expires : '{2:o}', AuthorizedResources : '{3}' }}", AccessToken, RefreshToken, Expires, string.Join(";", AuthorizedResources));
        }

        #region Equality methods

        protected bool Equals(ISession other)
        {
            return string.Equals(AccessToken, other.AccessToken)
                   && string.Equals(RefreshToken, other.RefreshToken)
                   && Expires.Equals(other.Expires)
                   && AuthorizedResources.SetEquals(other.AuthorizedResources);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ISession;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _accessToken.GetHashCode();
                hashCode = (hashCode*397) ^ _refreshToken.GetHashCode();
                hashCode = (hashCode*397) ^ _expires.GetHashCode();
                hashCode = (hashCode*397) ^ _authorizedResources.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Session left, Session right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Session left, Session right)
        {
            return !Equals(left, right);
        }

        #endregion

    }
}
