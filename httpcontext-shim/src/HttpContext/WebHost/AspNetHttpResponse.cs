using System.Web;

namespace HttpContextShim.WebHost
{
    public class AspNetHttpResponse : IHttpResponse
    {
        public AspNetHttpResponse(HttpResponse response)
        {
            Inner = response;
        }

        public AspNetHttpResponse(HttpResponseBase response)
        {
            Inner = response;
        }

        public object Inner { get; private set; }
    }
}