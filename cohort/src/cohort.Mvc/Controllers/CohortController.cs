using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using cohort.Configuration;
using cohort.Logging;
using cohort.Models;
using cohort.Mvc.Filters;
using cohort.Mvc.Security.CSRF;
using cohort.Services;
using cohort.ViewModels;
using linger;

namespace cohort.Mvc.Controllers
{
    // Remove duplication with redirects

    // Not working on AppHarbor due to broken CSRF tokens!
    //[RequireHttpsIfNotLocal]
    public class CohortController : Controller 
    {
        private readonly ActivationService _activation;
        private readonly AccountService _accounts;
        private readonly EmailService _email;
        private readonly StripeService _stripe;

        public CohortController()
        {
            _activation = Config.Container.Resolve<ActivationService>();//new ActivationService(user, auth, activation);
            _accounts = Config.Container.Resolve<AccountService>(); // (new AccountService(user, security, auth));
            _email = Config.Container.Resolve<EmailService>();////new EmailService(Cohort.Email.Engine, Cohort.Email.Provider, new EmailRepository());
            _stripe = Config.Container.Resolve<StripeService>();
        }
        
        [HttpGet]
        public ActionResult SignIn()
        {
            if (Request.IsLocal && Cohort.Site.LocalAuth.Enabled)
            {
                var user = Cohort.SignInLocal();
                if(user.IsAuthenticated())
                {
                    return RedirectToAction("Index", "Home", new { area = "User" });
                }
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "User" });
            }

            return View();
        }

        [AuthenticHttpPost]
        public ActionResult SignIn(SignIn model)
        {L
            if (Request.IsLocal && Cohort.Site.LocalAuth.Enabled)
            {
                Logger.Info("Signing in user locally");

                var user = Cohort.SignInLocal();
                if (user.IsAuthenticated())
                {
                    return RedirectToAction("Index", "Home", new { area = "User" });
                }
            }

            if (ModelState.IsValid)
            {
                Logger.Info("ModelState has all required information to sign in");

                var user = _accounts.GetUser(model.Email, activatedOnly: true);
                if (user == null || !_accounts.SignInIsValid(model, user))
                {
                    ModelState.AddModelError("", "Either the email address or the password you provided is incorrect.");
                    return View();
                }

                Logger.Info("Sign in credentials are valid for " + user.Identity);
                var ip = _accounts.GetIPAddress(Request);
                Logger.Info("Signing in from address " + ip);
                _accounts.SignIn(model.Email, ip, model.RememberMe);
                return RedirectToAction("Index", "Home", new { area = "User" });
            }

            Logger.Info("ModelState was not valid:");
            foreach(var state in ModelState)
            {
                Logger.Info(string.Format("{0}:{1}", state.Key, state.Value));
            }

            return View();
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }

        [AuthenticHttpPost]
        public ActionResult SignUp(SignUp model, string stripeToken)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.PasswordAgain)
                {
                    ModelState.AddModelError("password", "Both passwords entered must match.");
                }
                if (_accounts.GetUser(model.Email) != null)
                {
                    ModelState.AddModelError("email", "This username is already taken. Did you mean to sign in?");
                }

                if (Cohort.Site.Stripe.Enabled && !string.IsNullOrEmpty(stripeToken))
                {
                    // Successful credit capture is necessary in-process
                    if(!_stripe.SaveCustomerByToken(model.Email, stripeToken))
                    {
                        ModelState.AddModelError("", "We could not capture your credit card details. Please try again.");
                    }
                }

                if (ModelState.IsValid)
                {
                    _accounts.CreateNewSignUp(model, _accounts.GetIPAddress(Request));
                    var hash = _activation.StoreAuthenticationTicketForActivation(model.Email);

                    Func<bool> thing = () =>
                    {
                        _email.Send("Activation", new
                        {
                            To = model.Email,
                            From = Cohort.Site.Email.FromAddress,
                            Subject = Cohort.Site.Email.ActivationSubject,
                            ActivationLink = GetActivationLink(hash),
                            Cohort.Site.Membership.ActivationDays
                        });
                        return true;
                    };
                    thing.PerformAsync();
                }
            }

            return !ModelState.IsValid
                       ? (ActionResult) View(model)
                       : RedirectToAction("Index", "Home", new {area = "User"});
        }

        [UserFilter]
        public ActionResult SignOut(User user)
        {
            if (user != null)
            {
                _accounts.SignOut(user);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Activate(string id)
        {
            var hash = HttpUtility.HtmlDecode(id);
            var activated = _activation.ActivateAccount(hash);
            return View(activated ? "Activated" : "Activated_Error");
        }

        [AuthenticHttpPost]
        public ActionResult ResetPassword(ResetPassword model)
        {
            if(string.IsNullOrWhiteSpace(model.Email))
            {
                return View();
            }
            
            // "If the email provided is associated with an account, you will receive reset instructions there shortly. Thanks!"
            // The block below should be in a background task, to avoid revealing whether a user 
            // is a member of the site due to response time (if we check now)

            var hash = _activation.StoreAuthenticationTicketForReset(model.Email);
            _email.Send("ResetPassword", new
            {
                To = model.Email,
                From = Cohort.Site.Email.FromAddress,
                Subject = Cohort.Site.Email.ResetSubject,
                ResetPasswordLink = GetResetPasswordLink(hash),
                Cohort.Site.Membership.PasswordResetDays
            });

            return View("ResetPassword_EmailSent");
        }

        public ActionResult PlanSelect()
        {
            return View();
        }

        private string GetActivationLink(string hash)
        {
            return ControllerContext.RequestContext.GetLinkForRoute(new RouteValueDictionary
            {
                {"controller", "Cohort"},
                {"action", "Activate"},
                {"id", HttpUtility.HtmlEncode(hash)}
            });
        }

        private string GetResetPasswordLink(string hash)
        {
            return ControllerContext.RequestContext.GetLinkForRoute(new RouteValueDictionary
            {
                {"controller", "Cohort"},
                {"action", "ResetPassword"},
                {"id", HttpUtility.HtmlEncode(hash)}
            });
        }
    }
}
