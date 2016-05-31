using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace depot.Mvc
{
    public class DepotTempDataProvider : ITempDataProvider
    {
        private const string CacheKey = "Controller::TempData_";

        public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
        {
            var cacheKey = GetUserScopedKeyFor(CacheKey, controllerContext.HttpContext);
            var tempData = Depot.ObjectCache.Get<IDictionary<string, object>>(cacheKey);

            if (tempData == null) return new Dictionary<string, object>();
            Depot.ObjectCache.Remove<IDictionary<string, object>>(CacheKey);
            return tempData;
        }

        public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
        {
            var cacheKey = GetUserScopedKeyFor(CacheKey, controllerContext.HttpContext);
            Depot.ObjectCache.Set(cacheKey, values);
        }

        private static string GetUserScopedKeyFor(string baseKey, HttpContextBase httpContext)
        {
            if (httpContext == null) return null;
            var principal = httpContext.User;
            if (principal == null) return null;
            var key = string.Concat(baseKey, principal.Identity.Name);
            return key;
        }
    }
}
