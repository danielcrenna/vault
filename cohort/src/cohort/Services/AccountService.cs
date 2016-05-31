using System;
using System.Threading;
using System.Web;
using cohort.Models;
using cohort.ViewModels;

namespace cohort.Services
{
    public class AccountService
    {
        private readonly IUserRepository _user;
        private readonly ISecurityService _security;
        private readonly IAuthenticationService _auth;

        public AccountService(IUserRepository user, ISecurityService security, IAuthenticationService auth)
        {
            _user = user;
            _security = security;
            _auth = auth;
        }
        
        public void CreateNewSignUp(SignUp model, string ipAddress)
        {
            // Set the username to the email as well, to speed up searches, if username not used
            if (!Cohort.Site.Auth.Username)
            {
                model.Username = model.Email;
            }
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = _security.Hash(model.Password),
                IsActivated = false,
                LandingPageUrl = model.LandingPage,
                ReferrerUrl = model.RefererUrl,
                Culture = Thread.CurrentThread.CurrentUICulture.Name,
                JoinedOn = DateTime.Now,
                IPAddress = ipAddress
            };
            _user.Save(user);
        }

        public User GetUser(string email, bool activatedOnly = false)
        {
            return !string.IsNullOrWhiteSpace(email) ? _user.GetByEmail(email, activatedOnly) : null;
        }

        public bool SignInIsValid(SignIn model, User user)
        {
            return _security.ValidatePassword(model.Password, user.Password);
        }

        public void SignIn(string email, string ipAddress, bool rememberMe = false)
        {
            var user =_user.GetByEmail(email);
            if (user == null) return;
            user.IPAddress = ipAddress;
            user.SignedInOn = DateTime.Now;
            user.SignedOutOn = null;
            _user.Save(user);
            _auth.SignIn(email, rememberMe);
        }

        public void SignOut(User user)
        {
            user.SignedOutOn = DateTime.Now;
            _user.Save(user);
            _auth.SignOut();
        }

        public string GetIPAddress(HttpRequestBase request)
        {
            if (request == null || request.IsLocal) return "127.0.0.1";
            var proxy = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if(!string.IsNullOrWhiteSpace(proxy))
            {
                var ipArray = proxy.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if(ipArray.Length > 0)
                {
                    return ipArray[0];                    
                }
            }
            return request.UserHostAddress;
        }
    }
}