using System.Web.Mvc;

namespace cohort.Mvc.Demo.Areas.User.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
