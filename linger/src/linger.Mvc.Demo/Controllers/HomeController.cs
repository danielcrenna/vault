using System;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using DotLiquid;

namespace linger.Mvc.Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var path = HostingEnvironment.MapPath("~/Views/Home/Index.html");
            var index = System.IO.File.ReadAllText(path);
            var html = Template.Parse(index);
            
            return new ContentResult
            {
                Content = html.Render(Hash.FromAnonymousObject(new
                {
                    server = Environment.MachineName,
                    server_time = DateTime.Now
                })),
                ContentEncoding = Encoding.UTF8,
                ContentType = "text/html"
            };
        }
    }

}
