using System.IO;
using System.Text;
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
            var profilePics = Directory.EnumerateFiles(profilePicPath, "*.jpg");
            foreach (var profilePic in profilePics)
            {
                var title = Path.GetFileNameWithoutExtension(profilePic);
                title = SplitAtCaps(title);
                var picUri = "/content/ProfileAvatars/" + Path.GetFileName(profilePic);
                Clients.Caller.ReceiveContactSummary(new Models.ContactSummary { Title = title, Tags = new[] { "Work", "Prospect" }, PrimaryAvatar = picUri });
            }
        }

        private string SplitAtCaps(string value)
        {
            var sb = new StringBuilder();
            foreach (char c in value)
            {
                if (char.IsUpper(c)) sb.Append(' ');
                sb.Append(c);
            }
            return sb.ToString().Trim();
        }
    }
}