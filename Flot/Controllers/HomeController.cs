using System.IO;
using System.Net;
using System.Web.Mvc;
using metrics;
using metrics.Serialization;

namespace Flot.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult GetSample()
        {
            var content = Serializer.Serialize(Metrics.All);
            return Content(content);
        }
    }
}
