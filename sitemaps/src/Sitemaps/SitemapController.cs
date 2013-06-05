using System;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Sitemaps
{
    /// <summary>
    /// A controller for sending sitemaps to clients
    /// </summary>
    public class SitemapController : Controller
    {
        private const string ContentType = "application/xml";
        private const string IfNoneMatchHeader = "If-None-Match";

        private static string _hash;
        private readonly ISitemapService _sitemapService = new SitemapService();

        [HttpGet]
        public ActionResult Index(int? page, int? count)
        {
            var content = _sitemapService.GetSitemapXml(ControllerContext, page, count);
            var etag = MD5(content);

            if(BrowserIsRequestingFileIdentifiedBy(etag))
            {
                return NotModified();
            }

            _hash = etag;
            var cache = HttpContext.Response.Cache;
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetETag(_hash);
            
            return Content(content, ContentType, Encoding.UTF8);
        }

        public virtual ActionResult NotModified()
        {
            return StopWith(HttpStatusCode.NotModified);
        }

        private ActionResult StopWith(HttpStatusCode statusCode)
        {
            Response.StatusCode = (int)statusCode;
            Response.SuppressContent = true;
            Response.TrySkipIisCustomErrors = true;
            return null;
        }

        private bool BrowserIsRequestingFileIdentifiedBy(string etag)
        {
            if (Request.Headers[IfNoneMatchHeader] == null)
            {
                return false;
            }

            var ifNoneMatch = Request.Headers[IfNoneMatchHeader];
            return ifNoneMatch.Equals(etag, StringComparison.OrdinalIgnoreCase);
        }

        private static string MD5(string input)
        {
            var sb = new StringBuilder();
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                foreach (var hex in hash)
                {
                    sb.Append(hex.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
