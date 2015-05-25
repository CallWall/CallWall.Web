using CallWall.Web.Domain.Normalizers;

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
            var lcase = Handle.ToLowerInvariant();
            return GmailHandleNormalizer.Normalize(lcase);
        }        
    }
}