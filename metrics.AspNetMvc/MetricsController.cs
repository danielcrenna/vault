using System.Text;
using System.Web.Mvc;

namespace metrics.AspNetMvc
{
    public class MetricsController : Controller
    {
        public ContentResult Ping()
        {
            var result = new ContentResult
                             {
                                 Content = "pong",
                                 ContentType = "text/plain",
                                 ContentEncoding = Encoding.UTF8
                             };

            return result;
        }
    }
}