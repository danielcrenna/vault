using System.Text;
using System.Web.Mvc;

namespace ab.Lab.Controllers
{
    public class ABController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Choose(string experiment, int alternative)
        {
            var exp = ab.Experiments.Get(experiment);
            if(exp != null)
            {
                exp.Choose(alternative);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Conclude(string experiment, int alternative)
        {
            var exp = ab.Experiments.Get(experiment);
            if (exp != null)
            {
                exp.End();
            }
            return RedirectToAction("Index");
        }

        public ContentResult Experiments()
        {
            var result = new ContentResult
            {
                Content = ab.Experiments.Json(),
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8
            };
            return result;
        }
    }
}
