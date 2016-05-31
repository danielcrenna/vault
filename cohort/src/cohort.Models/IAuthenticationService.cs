using System.Web.Security;

namespace cohort
{
    public interface IAuthenticationService
    {
        void SignIn(string email, bool rememberMe);
        void SignOut();
        string Encrypt(FormsAuthenticationTicket ticket);
        FormsAuthenticationTicket Decrypt(string hash);
    }
}