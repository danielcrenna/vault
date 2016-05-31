using System.Web.Security;

namespace cohort
{
    public class FormsAuthenticationService : IAuthenticationService
    {
        public void SignIn(string email, bool rememberMe)
        {
            // Create the auth ticket manually, then store it somewhere we can periodically log people out when it is expired!
            // Note: Could be run in a web farm, so "peek" at the latest User first to see if they *need* to be logged out, before doing so when the ticket expires!
            // Also use the forms ticket timeout total minutes to do a friendly logout when the browser *is* open!

            //var ticket = new FormsAuthenticationTicket(
            //    1,
            //    email,
            //    DateTime.Now,
            //    DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
            //    false,
            //    "user,user1",
            //    FormsAuthentication.FormsCookiePath
            //);
            //var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            //var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            //{
            //    HttpOnly = true,
            //    Secure = FormsAuthentication.RequireSSL,
            //    Path = FormsAuthentication.FormsCookiePath,
            //    Domain = FormsAuthentication.CookieDomain
            //};
            //Response.AppendCookie(cookie);

            FormsAuthentication.SetAuthCookie(email, rememberMe);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public string Encrypt(FormsAuthenticationTicket ticket)
        {
            return FormsAuthentication.Encrypt(ticket);
        }

        public FormsAuthenticationTicket Decrypt(string hash)
        {
            return FormsAuthentication.Decrypt(hash);
        }
    }
}