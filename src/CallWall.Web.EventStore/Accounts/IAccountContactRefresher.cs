using System;
using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.EventStore.Accounts
{
    public interface IAccountContactRefresher
    {
        Task RequestRefresh(Guid userId, IAccount account, ContactRefreshTriggers triggeredBy);
    }
}
