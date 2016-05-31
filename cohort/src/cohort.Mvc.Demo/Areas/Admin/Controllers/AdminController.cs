using System.Web.Mvc;
using cohort.Models;
using cohort.Mvc.Filters;

namespace cohort.Mvc.Demo.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AdminController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Users()
        {
            // Server-driven templates are so 2011
            var model = _userRepository.GetAll();
            return View(model);
        }

        public ActionResult BrokenLinks()
        {
            return View();
        }

        public ActionResult Billing()
        {
            return View();
        }

        public ActionResult Jobs()
        {
            // Server-driven templates are so 2011
            //var repo = Linger.Backend().
            //return View(repo.GetAll());
            return View();
        }
    }
}