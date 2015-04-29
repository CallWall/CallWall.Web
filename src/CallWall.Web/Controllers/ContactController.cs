using System.Web.Mvc;

namespace CallWall.Web.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        public ActionResult Detail(string identifier)
        {
            return View();
        }

        public ActionResult Lookup(string[] keys)
        {
            //Perform lookup for Id
            //If single Id then redirect
            //Else Show Lookup View (which shows mini summaries of the matches, to click on to redirect to to the dash board)
            return View();
        }
    }
}
