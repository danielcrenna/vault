using System.Web.Mvc;

namespace cohort.Mvc.Filters
{
    public class RequireHttpsIfNotLocalAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }
            base.OnAuthorization(filterContext);
        }
    }
}