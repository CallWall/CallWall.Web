namespace CallWall.Web.Domain
{
    public sealed class ContactEmailAddress : ContactHandle
    {
        public ContactEmailAddress(string emailAddress, string qualifier)
        {
            HandleType = ContactHandleTypes.Email;
            Handle = emailAddress;
            Qualifier = qualifier;
        }

        public override string[] NormalizedHandle()
        {
            //TODO: add gmail normalization rules
            return new[] { Handle.ToLowerInvariant() };
        }        
    }
}