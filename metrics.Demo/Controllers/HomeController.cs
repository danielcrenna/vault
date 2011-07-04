using System.Web.Mvc;

namespace metrics.Demo.Controllers
{
    public class HomeController : Controller
    {
        private static int _hits;

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            _hits++;

            Metrics.Gauge(typeof (HomeController), "Index hits gauge", () => _hits);

            var counter = Metrics.Counter(typeof (HomeController), "Index hits counter");
            counter.Increment(1);

            var meter = Metrics.Timer(typeof (HomeController), "Index hits per second", TimeUnit.Seconds, TimeUnit.Seconds);
            
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
