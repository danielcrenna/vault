using System.Net.Http;

namespace HttpContextShim.SelfHost
{
    public class SelfHostHttpResponse : IHttpResponse
    {
        public SelfHostHttpResponse(HttpResponseMessage response)
        {
            Inner = response;
        }

        public object Inner { get; private set; }
    }
}