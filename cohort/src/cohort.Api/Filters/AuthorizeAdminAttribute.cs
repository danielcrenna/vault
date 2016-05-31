using System.Web.Http.Controllers;

namespace cohort.API.Filters
{
    /// <summary>
    /// Restricts access to API methods to logged in admin users
    /// </summary>
    public class AuthorizeAdminAttribute : InternalAuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (Cohort.User.IsAdmin())
            {
                return;
            }
            HandleUnauthorizedRequest(actionContext);
        }
    }
}