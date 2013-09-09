namespace CallWall.Web
{
    public interface IPersonalIdentifier
    {
        IAccountConfiguration Account { get; }
        string IdentifierType { get; }
        string Value { get; }
    }
}