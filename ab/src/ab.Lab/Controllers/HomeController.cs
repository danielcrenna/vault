using System.Web.Mvc;

namespace ab.Lab.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ClicketyClick()
        {
            M.Track("Button clicks");

            return View();
        }
    }
}
