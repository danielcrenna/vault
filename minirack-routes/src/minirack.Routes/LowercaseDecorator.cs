using System.Web.Routing;

namespace minirack.Routes
{
    public class LowercaseDecorator : RouteDecoratorBase<LowercaseDecorator>
    {
        public LowercaseDecorator(RouteBase route) : base(route)
        {

        }

        public override VirtualPathData GetVirtualPath(RequestContext context, RouteValueDictionary values)
        {
            var result = Route.GetVirtualPath(context, values);
            if (result != null && result.VirtualPath != null)
            {
                result.VirtualPath = result.VirtualPath.ToLowerInvariant();
            }
            return result;
        }
    }
}
