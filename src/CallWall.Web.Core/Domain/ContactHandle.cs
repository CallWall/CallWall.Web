namespace CallWall.Web.Domain
{
    public abstract class ContactHandleTypes
    {
        public static readonly string Phone = "Phone";
        public static readonly string Email = "Email";
    }

    public class ContactHandle
    {
        /// <summary>
        /// THe type of handle this represents e.g. PhoneNumber, Email, UserId
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
                return ((HandleType != null ? HandleType.GetHashCode() : 0)*397) ^ (Handle != null ? Handle.GetHashCode() : 0);
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

    public sealed class ContactEmailAddress : ContactHandle
    {
        public ContactEmailAddress(string emailAddress, string qualifier)
        {
            HandleType = ContactHandleTypes.Email;
            Handle = emailAddress;
            Qualifier = qualifier;
        }
    }

    public sealed class ContactPhoneNumber : ContactHandle
    {
        public ContactPhoneNumber(string phoneNumber, string qualifier)
        {
            HandleType = ContactHandleTypes.Phone;
            Handle = phoneNumber;
            Qualifier = qualifier;
        }
    }
}