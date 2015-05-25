using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Protocols;
using CallWall.Web.Domain;
using CallWall.Web.Providers;
using Microsoft.Ajax.Utilities;

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

            if (contacts.Count == 0)
                return View("NoMatches", (object)GenerateSearchUrl(handles.Select(h=>h.Handle)));
            if (contacts.Count == 1)
                return new RedirectResult("../Detail/?id=" + contacts[0].Id);
            return View(contacts);
        }

        private string GenerateSearchUrl(IEnumerable<string> terms)
        {
            var tokenisedTerms = terms.Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Select(t => t.Replace("\"", string.Empty))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => string.Format("\"{0}\"", t));
            var queryString = Server.UrlEncode(string.Join(" OR ", tokenisedTerms));
            return "https://www.google.com/search?q=" + queryString;
        }
    }
}
