using System.IO;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CallWall.Web.Hubs
{
    [HubName("contacts")]
    public class ContactsHub : Hub
    {
        public void RequestContactSummaryStream()
        {
            var profilePicPath = Path.Combine(HttpRuntime.AppDomainAppPath, "content/ProfileAvatars/");
            var profilePics = Directory.EnumerateFiles(profilePicPath);
            foreach (var profilePic in profilePics)
            {
                var title = Path.GetFileNameWithoutExtension(profilePic);
                var picUri = "/content/ProfileAvatars/" + Path.GetFileName(profilePic);
                Clients.Caller.ReceiveContactSummary(new Models.ContactSummary { Title = title, Tags = new[] { "Work", "Prospect" }, PrimaryAvatar = picUri });
            }
        }
    }
}