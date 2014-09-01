using System.Threading.Tasks;

namespace CallWall.Web.EventStore.Tests
{
    public interface IContactsRepository
    {
        Task RequestRefreshFor(IAccount account);
    }
}