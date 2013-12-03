namespace CallWall.Web.Providers
{
   public static class SecurityExtensions
    {
        //public static ISession ToSession(this IPrincipal user)
        //{
        //    var principal = user as ClaimsPrincipal;
        //    if (principal == null) return null;

        //    string provider = principal.FindFirst(SecurityProvider.ProviderTypeKey).Value;
        //    string accessToken = principal.FindFirst(SecurityProvider.AccessTokenTypeKey).Value;
        //    string refreshToken = principal.FindFirst(SecurityProvider.RefreshTokenTypeKey).Value;
        //    string strExpiry = principal.FindFirst(SecurityProvider.ExpiryTypeKey).Value;
        //    var expiry = DateTimeOffset.ParseExact(strExpiry, "o", CultureInfo.InvariantCulture);
        //    var resources = principal.FindAll(SecurityProvider.ResourceTypeKey)
        //                             .Select(c => new Uri(c.Value));
        //    var session = new Session(accessToken, refreshToken, expiry, resources);

        //    return session;
        //}
    }
}