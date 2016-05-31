using System;
using System.Web.Mvc;
using depot.Mvc.Example.Models;

namespace depot.Mvc.Example.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string cacheKey)
        {
            return View();
        }

        [Depot]
        public ActionResult SimpleInteger(string cacheKey, DepotCache cache)
        {
            var value = cache.Get(cacheKey, GetRandomNumber());
            return View(value);
        }

        private static Func<IntegerModel> GetRandomNumber()
        {
            var rnd = new Random();
            return () => new IntegerModel {Value = rnd.Next()};
        }
    }
}
