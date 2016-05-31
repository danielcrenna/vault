using System.Web.Mvc;
using cohort.Mvc.Filters;

namespace cohort.Mvc.Demo.Areas.Admin.Controllers
{
    [AuthorizeSuperUser]
    public class SuperUserController : Controller
    {
        public ActionResult Logs()
        {
            return View();
        }

        public ActionResult Configuration()
        {
            return View();
        }
    }
}