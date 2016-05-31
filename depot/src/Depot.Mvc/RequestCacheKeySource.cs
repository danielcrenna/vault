using System.Text;
using System.Web.Routing;

namespace depot.Mvc
{
    /// <summary>
    /// Builds a cache key based on the incoming HTTP request.
    /// </summary>
    public class RequestCacheKeySource : ICacheKeySource
    {
        private readonly RequestContext _context;
        public RequestCacheKeySource(RequestContext context)
        {
            _context = context;
        }
        public string GetKey()
        {
            var rd = _context.RouteData;
            var sb = new StringBuilder("request->");
            foreach(var item in rd.DataTokens)
            {
                sb.Append(item.Key)
                    .Append(":")
                    .Append(item.Value);
            }
            sb.Append(",");
            sb.Append(_context.HttpContext.Request.QueryString);
            return sb.ToString();
        }
    }
}