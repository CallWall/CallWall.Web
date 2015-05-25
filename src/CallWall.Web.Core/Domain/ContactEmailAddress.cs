using System.Text.RegularExpressions;

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

    sealed class GmailHandleNormalizer
    {
        private static readonly Regex DomainRegex = new Regex(@"googlemail\.com", RegexOptions.Compiled);
        private static readonly Regex AliasRegex = new Regex(@"(?=.*)\+.*(?=@gmail.com)", RegexOptions.Compiled);
        private static readonly Regex LocalPartRegex = new Regex(@".*(?=@gmail.com)", RegexOptions.Compiled);
        //assumes handle is lower case already.
        public static string[] Normalize(string handle)
        {
            var correction = DomainRegex.Replace(handle, "gmail.com");
            correction = AliasRegex.Replace(correction, string.Empty);
            correction = LocalPartRegex.Replace(correction, match=>match.Value.Replace(".", string.Empty));

            return new[] {correction};
        }
    }
}