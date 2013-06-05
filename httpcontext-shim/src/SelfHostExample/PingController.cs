using System.Web.Http;
using HttpContextShim;

namespace SelfHostExample
{
    public class PingController : ApiController 
    {
        public IHttpContext Get()
        {
            var context = HttpContext.Current;
            context.Items.Add("response", "pong");
            return context;
        }
    }
}
