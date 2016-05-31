using System.Web.Mvc;

namespace hollywood
{
    public class LaunchController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}