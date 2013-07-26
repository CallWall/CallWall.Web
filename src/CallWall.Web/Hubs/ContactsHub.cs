using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contacts")]
    public class ContactsHub : Hub
    {
        public IEnumerable<string> GetContacts()
        {
            yield return "Adam Ant";
            yield return "Barry Balls";
            yield return "Charlie Chaplin";
            yield return "Dirk Drexler";
        }

        public void requestContactStream()
        {
            Thread.Sleep(1000);
            Clients.Caller.receiveContact("Adam Ant2");
            Thread.Sleep(1000);
            Clients.Caller.receiveContact("Barry Balls2");
            Thread.Sleep(1000);
            Clients.Caller.receiveContact("Charlie Chaplin2");
            Thread.Sleep(1000);
            Clients.Caller.receiveContact("Dirk Drexler2");
        }
        
        public void RequestContactSummaryStream()
        {
            Clients.Caller.ReceiveContactSummary(new Models.ContactSummary { Title = "Adam Ant3", Tags = new[] { "Work", "Prospect" } });
            Clients.Caller.ReceiveContactSummary(new Models.ContactSummary { Title = "Barry Balls3", Tags = new[] { "Family", "Rugby" } });
            Clients.Caller.ReceiveContactSummary(new Models.ContactSummary { Title = "Charlie Chaplin3", Tags = new[] { "Theatre" } });
            Clients.Caller.ReceiveContactSummary(new Models.ContactSummary { Title = "Dirk Drexler3", Tags = new[] { "Work", "Prospect" } });
        }
    }


}