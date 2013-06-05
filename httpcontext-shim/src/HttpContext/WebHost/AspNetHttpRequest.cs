using System.Web;

namespace HttpContextShim.WebHost
{
    public class AspNetHttpRequest : IHttpRequest
    {
        public AspNetHttpRequest(HttpRequest request)
        {
            IsLocal = request.IsLocal;
            UserHostAddress = request.UserHostAddress;
            Inner = request;
        }

        public AspNetHttpRequest(HttpRequestBase request)
        {
            IsLocal = request.IsLocal;
            UserHostAddress = request.UserHostAddress;
            Inner = request;
        }

        public bool IsLocal { get; private set; }
        public string UserHostAddress { get; private set; }
        public object Inner { get; private set; }
    }
}