using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;

namespace HttpContextShim.SelfHost
{
    public class SelfHostHttpContext : HttpContext
    {
        public SelfHostHttpContext()
        {
            Timestamp = DateTime.Now;
            Items = new ConcurrentDictionary<string, object>();
            User = Thread.CurrentPrincipal;
            Inner = this;
        }

        public SelfHostHttpContext(HttpRequestMessage request) : this()
        {
            Request = new SelfHostHttpRequest(request);
            request.Properties.Add("MS_HttpContext", this);
        }

        public void SetResponse(HttpResponseMessage response)
        {
            Response = new SelfHostHttpResponse(response);
        }
    }
}