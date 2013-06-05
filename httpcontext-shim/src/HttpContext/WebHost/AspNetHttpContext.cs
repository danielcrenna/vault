using System.Web;

namespace HttpContextShim.WebHost
{
    public class AspNetHttpContext : HttpContext
    {
        public AspNetHttpContext(System.Web.HttpContext context)
        {
            Timestamp = context.Timestamp;
            Request = new AspNetHttpRequest(context.Request);
            Response = new AspNetHttpResponse(context.Response);
            Items = context.Items;
            User = context.User;
            Inner = context;
        }

        public AspNetHttpContext(HttpContextBase context)
        {
            Timestamp = context.Timestamp;
            Request = new AspNetHttpRequest(context.Request);
            Response = new AspNetHttpResponse(context.Response);
            Items = context.Items;
            Inner = context;
        }
    }
}