using System.Web.Mvc;

namespace cohort.Mvc.Filters
{
    /// <summary>
    /// Injects the user into action methods.
    /// </summary>
    public class UserFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            const string key = "user";
            if (filterContext.ActionParameters.ContainsKey(key))
            {
                filterContext.ActionParameters[key] = Cohort.User;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}