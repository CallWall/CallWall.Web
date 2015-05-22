using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Web.Mvc;
using CallWall.Web.Domain;
using CallWall.Web.Providers;

namespace CallWall.Web.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly IContactRepository _contactRepository;
        private readonly ILoginProvider _loginProvider;

        public ContactController(IContactRepository contactRepository, ILoginProvider loginProvider)
        {
            _contactRepository = contactRepository;
            _loginProvider = loginProvider;
        }

        public ActionResult Detail(string identifier)
        {
            return View();
        }

        public async Task<ActionResult> Lookup(string[] phone, string[] email, string[] handle)
        {
            var userId = User.UserId();
            var user = await _loginProvider.GetUser(userId);

            //Perform lookup for Id

            var handles = (phone ?? Enumerable.Empty<string>()).Select<string, ContactHandle>(ph => new ContactPhoneNumber(ph, null))
                .Concat((email ?? Enumerable.Empty<string>()).Select(e => new ContactEmailAddress(e, null)))
                //.Concat((handle ?? Enumerable.Empty<string>()).Select(h=>new ContactHandle(h)))
                .ToArray();

            var contacts = await _contactRepository.LookupContactByHandles(user, handles)
                .ToList()
                .ToTask();

            if (contacts.Count == 1)
                return new RedirectResult("../Detail/?id=" + contacts[0].Id);
            else
                return View(contacts);
        }
    }
}
