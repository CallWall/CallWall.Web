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

        public async Task<ActionResult> Lookup(string[] keys)
        {
            var userId = User.UserId();
            var user = await _loginProvider.GetUser(userId);

            //Perform lookup for Id
            var contacts = await _contactRepository.LookupContactByKey(user, keys)
                .ToList()
                .ToTask();

            if (contacts.Count == 1)
                return new RedirectResult("../Detail/?id=" + contacts[0].Id);
            else
                return View(contacts);
        }
    }
}
