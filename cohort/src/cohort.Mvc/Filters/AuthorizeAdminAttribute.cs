using System;
using System.Web;
using System.Web.Mvc;

namespace cohort.Mvc.Filters
{
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            var user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }

            return Cohort.User != null && Cohort.User.IsAdmin();
        }
    }
}