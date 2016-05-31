using System.Web.Mvc;

namespace cohort.Mvc.Filters
{
    /// <summary>
    /// Injects the available profile into action methods. If the user is not authenticated, an anonymous profile is used.
    /// </summary>
    public class ProfileFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            const string key = "profile";
            if (filterContext.ActionParameters.ContainsKey(key))
            {
                filterContext.ActionParameters[key] = Cohort.Profile;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}