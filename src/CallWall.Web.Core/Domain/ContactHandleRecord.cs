using System;
using System.Collections.Generic;

namespace CallWall.Web.Domain
{
    public sealed class ContactHandleRecord
    {
        private static readonly Dictionary<string, Func<ContactHandleRecord, ContactHandle>> ContactHandleMap = new Dictionary<string, Func<ContactHandleRecord, ContactHandle>>
        {
            {ContactHandleTypes.Email, rec=>new ContactEmailAddress(rec.Handle, rec.Qualifier) },
            {ContactHandleTypes.Phone, rec=>new ContactPhoneNumber(rec.Handle, rec.Qualifier) }
        };

        public ContactHandleRecord()
        {
        }

        public ContactHandleRecord(ContactHandle source)
        {
            HandleType = source.HandleType;
            Handle = source.Handle;
            Qualifier = source.Qualifier;
        }

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

        public ContactHandle ToContactHandle()
        {
            return ContactHandleMap[HandleType](this);
        }

        #region Equality operators

        private bool Equals(ContactHandleRecord other)
        {
            return string.Equals(HandleType, other.HandleType) && string.Equals(Handle, other.Handle) && string.Equals(Qualifier, other.Qualifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ContactHandleRecord && Equals((ContactHandleRecord)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (HandleType != null ? HandleType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Handle != null ? Handle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Qualifier != null ? Qualifier.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ContactHandleRecord left, ContactHandleRecord right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ContactHandleRecord left, ContactHandleRecord right)
        {
            return !Equals(left, right);
        }

        #endregion Equality operators
    }
}