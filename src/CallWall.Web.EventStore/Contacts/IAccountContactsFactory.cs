using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Contacts
{
    public interface IAccountContactsFactory
    {
        AccountContacts Create(IAccount account);
    }
}