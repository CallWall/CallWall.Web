namespace CallWall.Web
{
    public interface IAccountFactory
    {
        IAccount Create(string accountId, string provider, string displayName, ISession session);
    }
}