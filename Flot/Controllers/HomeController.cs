using System.Web.Mvc;
using metrics;
using metrics.Serialization;

namespace Flot.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult GetSample()
        {
            var content = Serializer.Serialize(Metrics.AllSorted);
            return Content(content);
        }
    }
}
