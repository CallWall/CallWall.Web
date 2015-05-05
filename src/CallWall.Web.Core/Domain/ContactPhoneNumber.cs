namespace CallWall.Web.Domain
{
    public sealed class ContactPhoneNumber : ContactHandle
    {
        public ContactPhoneNumber(string phoneNumber, string qualifier)
        {
            HandleType = ContactHandleTypes.Phone;
            Handle = phoneNumber;
            Qualifier = qualifier;
        }

        public override string[] NormalizedHandle()
        {
            return PhoneNumber.Parse(Handle);
        }
    }
}