using System;
using System.Text;
using System.Web.Mvc;

namespace linger.Mvc.Demo.Controllers
{
    public class LingerController : Controller 
    {
        // GET linger
        public ActionResult Index()
        {
            JsonSerializer.Serialize(new {environment = Environment.MachineName});
            return new ContentResult
            {
                Content = Linger.Dump(),
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json"
            };
        }

        // GET linger/jobs
        public ActionResult Jobs()
        {
            return new ContentResult
            {
                Content = Linger.Dump(),
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json"
            };
        }
    }
}