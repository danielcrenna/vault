using System.Web.Mvc;
using cohort.Logging;
using cohort.Mvc.Filters;
using cohort.Mvc.Security.CSRF;
using cohort.Services;
using cohort.ViewModels;

namespace cohort.Mvc.Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProfileRepository _profile;
        private readonly IEmailService _emailService;

        public HomeController(IProfileRepository profile, IEmailService emailService)
        {
            _profile = profile;
            _emailService = emailService;
        }

        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost, AuthenticHttpPost]
        public ActionResult Contact(SystemContact model)
        {
            // Make this internal and use a canned view model to make it happen (public email service should be canned)???
            // i.e. Cohort.Send(Email.Contact, model);
            _emailService.Send("Contact", new
            {
                // From the system
                To = Cohort.Site.Email.FromAddress,              // This should be to any recipients in the recipient list
                From = Cohort.Site.Email.FromAddress,
                Subject = Cohort.Site.Email.ContactSubject,

                // From the contacter
                model.ContactMessage,
                model.ContactSubject,
                model.ContactEmail
            });
            return View();
        }

        [ProfileFilter]
        public ActionResult Theme(Profile profile, string theme)
        {
            Logger.Info("Current theme is " + profile.Theme);
            profile.Theme = theme;
            _profile.Save(profile);
            Logger.Info("Theme changed to " + profile.Theme);

            var referrer = Request.UrlReferrer;
            return Redirect(referrer.ToString());
        }
    }
}
