using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace minirack.Routes
{
    public abstract class RouteDecoratorBase<T> : RouteBase, IRouteWithArea where T : RouteDecoratorBase<T>
    {
        protected RouteBase Route;

        protected RouteDecoratorBase(RouteBase route)
        {
            Route = route;
        }

        public RouteBase InnerRoute
        {
            get { return Route; }
        }

        public string Area
        {
            get
            {
                var r = Route;
                while (r is T)
                {
                    r = ((T)r).InnerRoute;
                }
                var s = GetAreaToken(r);
                return s;
            }
        }

        public override RouteData GetRouteData(HttpContextBase context)
        {
            return Route.GetRouteData(context);
        }

        private static string GetAreaToken(RouteBase r)
        {
            var route = r as Route;
            if (route != null && route.DataTokens != null && route.DataTokens.ContainsKey("area"))
            {
                return (route.DataTokens["area"] as string);
            }
            return null;
        }
    }
}
