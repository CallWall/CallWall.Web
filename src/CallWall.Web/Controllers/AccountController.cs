using System.Web.Mvc;

namespace CallWall.Web.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Register()
        {
            return View();
        }

        public ActionResult LogIn()
        {
            return View();
        }

        public ActionResult Manage()
        {
            return View();
        }


        public ActionResult LogOff()
        {
            return new RedirectResult("/");
        }
    }
}
