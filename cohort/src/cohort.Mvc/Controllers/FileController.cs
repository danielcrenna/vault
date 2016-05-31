using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using cohort.Extensions;
using depot;

namespace cohort.Mvc.Controllers
{
    // Needs to be able to be configurable (enabled, what directories, etc...)

    public class FileController : Controller 
    {
        private const string LastModifiedSinceHeader = "If-Modified-Since";
        private const string IfNoneMatchHeader = "If-None-Match";
        private const string DeflateTag = "-deflate";
        private const string GzipTag = "-gzip";

        [HttpGet]
        public ActionResult Serve(string pathInfo)
        {
            var context = ControllerContext.HttpContext;
            var fileName = Path.GetFileName(pathInfo);
            var absolutePath = Server.MapPath(fileName);
            if (!System.IO.File.Exists(absolutePath))
            {
                return NotFound();
            }

            // A better test of uniqueness than modified date
            var etag = GenerateETag(ControllerContext, absolutePath);
            if (etag.StartsWith("404"))
            {
                // 404 (NotFound)
                return NotFound();
            }

            if (BrowserIsRequestingFileIdentifiedBy(etag))
            {
                // 304 (If-None-Match)
                return NotModified();
            }

            if (!System.IO.File.Exists(absolutePath))
            {
                // 404 (NotFound)
                return NotFound();
            }

            var lastModified = new FileInfo(absolutePath).LastWriteTime;
            if (BrowserIsRequestingFileUnmodifiedSince(lastModified))
            {
                // 304 (If-Last-Modified)
                return NotModified();
            }
            
            // 200 - OK
            CacheOnClient(context, etag, lastModified);
            var content = CacheOrFetchFromServer(absolutePath);
            var contentType = MimeType(absolutePath);
            var stream = new MemoryStream(content);
            var result = new FileStreamResult(stream, contentType);
            return result;
        }

        private FileStreamResult NotModified()
        {
            Response.StatusCode = (int)HttpStatusCode.NotModified;
            Response.SuppressContent = true;
            Response.TrySkipIisCustomErrors = true;
            return null;
        }

        private FileStreamResult NotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
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

        private bool BrowserIsRequestingFileUnmodifiedSince(DateTime lastModified)
        {
            if (Request.Headers[LastModifiedSinceHeader] == null)
            {
                return false;
            }

            // Header values may have additional attributes separated by semi-colons
            var ifModifiedSince = Request.Headers[LastModifiedSinceHeader];
            if (ifModifiedSince.IndexOf(";", StringComparison.Ordinal) > -1)
            {
                ifModifiedSince = ifModifiedSince.Split(';').First();
            }

            // Get the dates for comparison; truncate milliseconds in date if needed
            var sinceDate = Convert.ToDateTime(ifModifiedSince).ToUniversalTime();
            var fileDate = lastModified.ToUniversalTime();
            if (sinceDate.Millisecond.Equals(0))
            {
                fileDate = new DateTime(fileDate.Year,
                                        fileDate.Month,
                                        fileDate.Day,
                                        fileDate.Hour,
                                        fileDate.Minute,
                                        fileDate.Second,
                                        0);
            }

            return fileDate <= sinceDate;
        }

        private static byte[] CacheOrFetchFromServer(string absolutePath)
        {
            var content = Depot.ContentCache.Get(absolutePath) ?? CacheOnServer(absolutePath, absolutePath);
            return content;
        }
        
        private static byte[] CacheOnServer(string absolutePath, string dependencyPath)
        {
            var contentDependency = new DefaultFileCacheDependency();
            contentDependency.FilePaths.Add(dependencyPath);
            
            var content = System.IO.File.ReadAllBytes(absolutePath);
            Depot.ContentCache.Add(absolutePath, content, contentDependency);
            return content;
        }
        
        private static void CacheOnClient(HttpContextBase context, string etag, DateTime lastModified)
        {
            var futureDate = new TimeSpan(365, 0, 0, 0);
            var expires = DateTime.UtcNow.Add(futureDate);

            var cachePolicy = context.Response.Cache;
            cachePolicy.SetCacheability(HttpCacheability.Public);
            
            // Forces a round-trip to the server to check for staleness; tells clients they can't serve stale data
            // This could become a performance bottleneck and may need to be configurable higher up
            cachePolicy.AppendCacheExtension("must-revalidate, proxy-revalidate");
            
            cachePolicy.SetExpires(expires);
            cachePolicy.SetMaxAge(futureDate);
            cachePolicy.SetLastModified(lastModified);
            cachePolicy.SetETag(etag);
        }

        private static string MimeType(string path)
        {
            var value = Path.GetExtension(path) ?? "";
            var extension = value.ToLowerInvariant();
            switch (extension)
            {
                case ".js":
                    return "text/javascript";
                case ".css":
                    return "text/css";
                case ".jpg":
                    return "image/jpeg";
                case ".jpeg":
                    return "image/jpg";
                case ".png":
                    return "image/png";
                case ".bmp":
                    return "image/bmp";
                case ".gif":
                    return "image/gif";
            }
            return "application/octet-stream";
        }

        public string GenerateETag(ControllerContext context, string absolutePath)
        {
            // If the content is cached, so is the ETag
            var etag = Depot.StringCache.Get(
                string.Format("etag_{0}", absolutePath)
                );

            if (!string.IsNullOrWhiteSpace(etag))
            {
                return etag;
            }

            if (!System.IO.File.Exists(absolutePath))
            {
                return "\"404\"";
            }

            etag = BuildETagFromPath(context, absolutePath);
            if (etag != null)
            {
                var etagDependency = new DefaultFileCacheDependency();
                etagDependency.FilePaths.Add(absolutePath);
                Depot.StringCache.Add(string.Format("etag_{0}", absolutePath), etag, etagDependency);
            }

            return etag;
        }

        public string BuildETagFromPath(ControllerContext context, string absolutePath)
        {
            var sb = new StringBuilder("\"");
            using (var stream = new FileStream(absolutePath, FileMode.Open))
            {
                sb.Append(stream.MD5());
            }
            if (HasGzipFilter(context))
            {
                sb.Append(GzipTag);
            }
            else if (HasDeflateFilter(context))
            {
                sb.Append(DeflateTag);
            }
            sb.Append("\"");
            return sb.ToString();
        }

        public static bool HasGzipFilter(ControllerContext context)
        {
            // You can't inspect the response headers directly in IIS6 / Cassini
            return Convert.ToBoolean(context.HttpContext.Items["gzipped"] ?? "false");
        }

        public static bool HasDeflateFilter(ControllerContext context)
        {
            // You can't inspect the response headers directly in IIS6 / Cassini
            return Convert.ToBoolean(context.HttpContext.Items["deflated"] ?? "false");
        }
    }
}
