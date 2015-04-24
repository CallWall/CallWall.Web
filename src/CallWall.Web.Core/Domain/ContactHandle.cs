namespace CallWall.Web.Domain
{
    public abstract class ContactHandle
    {
        /// <summary>
        /// The type of handle this represents e.g. PhoneNumber, Email, UserId
        /// </summary>
        public string HandleType { get; set; }
        /// <summary>
        /// The value of the handle e.g. 021-1234-4567, Bob@mail.com, @MyTwitterAccount
        /// </summary>
        public string Handle { get; set; }
        /// <summary>
        /// The qualifier of the handle value e.g. Home, Office, Mobile
        /// </summary>
        public string Qualifier { get; set; }

        /// <summary>
        /// The normalized values of the handle. e.g. +64 21-1234-4567 might be normalized to ["02112344567", "+642112344567"], and "Bob.Dodds@gmail.com" might be ["bobdodds@gmail.com"]
        /// </summary>
        /// <returns>An array of the normalized values for the handle</returns>
        public abstract string[] NormalizedHandle();

        protected bool Equals(ContactHandle other)
        {
            return string.Equals(HandleType, other.HandleType) && string.Equals(Handle, other.Handle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ContactHandle;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((HandleType != null ? HandleType.GetHashCode() : 0) * 397) ^ (Handle != null ? Handle.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ContactHandle left, ContactHandle right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ContactHandle left, ContactHandle right)
        {
            return !Equals(left, right);
        }
    }
}