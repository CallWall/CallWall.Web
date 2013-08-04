namespace CallWall.Web.Models
{
    public interface IPersonalIdentifier
    {
        IProviderDescription Provider { get; }
        string IdentifierType { get; }
        string Value { get; }
    }
}