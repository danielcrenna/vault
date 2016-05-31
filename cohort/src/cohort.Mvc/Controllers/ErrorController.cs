using System.Web.Mvc;

namespace cohort.Mvc.Controllers
{
    public class ErrorController : Controller
    {
        public ViewResult NotFound()
        {
            return View();
        }

        public ViewResult Unknown()
        {
            return View();
        }
    }
}