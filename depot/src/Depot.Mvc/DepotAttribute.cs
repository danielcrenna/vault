using System;
using System.Web.Mvc;

namespace depot.Mvc
{
    // Scenario where result caching is inappropriate
    //[Depot]
    //public ActionResult GetFoo(string id, ICache cache, string cacheKey)
    //{
    //    var model = return cache.Get<Foo>(cacheKey,
    //        () => repository.GetFoo(id),
    //        a => GetFooDependencies(a)
    //        );
    //
    //    return View(model);
    //}

    // Cache must be private to a user if the method needs authorization
    // Cache must be sensitive to query strings (should it still keep objects cached?)

    public class DepotAttribute : ActionFilterAttribute 
    {
        private const string CacheKey = "__Depot__CacheKey__";
        private readonly CacheKeySource _source;
        
        public DepotAttribute(CacheKeySource source = CacheKeySource.Request)
        {
            _source = source;
            Order = 10000;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            const string cacheKey = "cache";
            if (filterContext.ActionParameters.ContainsKey(cacheKey))
            {
                filterContext.ActionParameters[cacheKey] = new DepotCache();
            }

            var key = CreateAndStoreCacheByKey(filterContext);
            var existing = Depot.ObjectCache.Get(key);
            if(existing != null)
            {
                // Setting the result here will skip execution of the action
                filterContext.Result = existing as ActionResult;
                return;
            }
            filterContext.HttpContext.Items.Add(CacheKey, key);

            base.OnActionExecuting(filterContext);
        }
        
        private string CreateAndStoreCacheByKey(ActionExecutingContext filterContext)
        {
            var source = GetKeyFromSource(filterContext);
            var key = source.GetKey();
            const string cacheKey = "cacheKey";
            if (filterContext.ActionParameters.ContainsKey(cacheKey))
            {
                filterContext.ActionParameters[cacheKey] = key;
            }
            return key;
        }

        private ICacheKeySource GetKeyFromSource(ControllerContext filterContext)
        {
            ICacheKeySource source;
            switch (_source)
            {
                case CacheKeySource.Request:
                    source = new RequestCacheKeySource(filterContext.RequestContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return source;
        }

        //public override void OnResultExecuted(ResultExecutedContext filterContext)
        //{
        //    //var value = filterContext.Controller.ViewData.Model;
        //    var key = filterContext.HttpContext.Items[CacheKey];
        //    if(key == null) return;
        //    var value = filterContext.Result;
        //    if(value != null)
        //    {
        //        Depot.ObjectCache.Set(key.ToString(), value);
        //    }
        //    base.OnResultExecuted(filterContext);
        //}
    }

    public enum CacheKeySource
    {
        Request
    }
    
    public interface ICacheKeySource
    {
        string GetKey();
    }
}
