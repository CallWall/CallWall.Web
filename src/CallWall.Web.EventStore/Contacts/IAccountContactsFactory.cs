namespace CallWall.Web.EventStore.Contacts
{
    public interface IAccountContactsFactory
    {
        AccountContacts Create(IAccountData accountData);
    }
}