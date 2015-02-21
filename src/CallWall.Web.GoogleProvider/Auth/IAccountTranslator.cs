using CallWall.Web.Domain;

namespace CallWall.Web.GoogleProvider.Auth
{
    interface IAccountTranslator
    {
        IAccount TranslateToAccount(string response, ISession session);
    }
}