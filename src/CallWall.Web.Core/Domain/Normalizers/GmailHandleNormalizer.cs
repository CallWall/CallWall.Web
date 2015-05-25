using System.Text.RegularExpressions;

namespace CallWall.Web.Domain.Normalizers
{
    static class GmailHandleNormalizer
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