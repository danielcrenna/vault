using System.Web.Mvc;

namespace cohort.Mvc
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //http://stackoverflow.com/questions/6508415/application-error-not-firing-when-customerrors-on
            //filters.Add(new HandleErrorAttribute());
            //filters.Add(new SecureCookiesFilterAttribute());
        }
    }
}