using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using cohort.API.Streaming;

namespace cohort.API.Controllers
{
    // - Authenticate / pair with the logged in user

    public class UploadController : ApiController
    {
        public Task<HttpResponseMessage> Post()
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var streamProvider = new S3MultipartStreamProvider();
                var task = Request.Content.ReadAsMultipartAsync(streamProvider).ContinueWith(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }

                    // Save media details, and possibly provide a link later, but just post back for now
                    var response = Request.CreateResponse(HttpStatusCode.Moved);
                    response.Headers.Location = new Uri(Request.Headers.Referrer.ToString());
                    return response;
                });
                return task;
            }
            throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted"));
        }
    }
}

