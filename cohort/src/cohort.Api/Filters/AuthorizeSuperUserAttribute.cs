using System.Web.Http.Controllers;

namespace cohort.API.Filters
{
    /// <summary>
    /// Restricts access to API methods to the super user
    /// </summary>
    public class AuthorizeSuperUserAttribute : InternalAuthorizeAttribute 
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (Cohort.User.IsSuperUser())
            {
                return;
            }
            HandleUnauthorizedRequest(actionContext);
        }
    }
}
