using System;
using System.IO;
using System.Net;

namespace Hammock.Web.Mocks
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class MockHttpWebRequest : WebRequest
    {
        private readonly Uri _requestUri;

        public virtual HttpStatusCode ExpectStatusCode { get; protected internal set; }
        public virtual string ExpectStatusDescription { get; protected internal set; }
        public virtual WebHeaderCollection ExpectHeaders { get; protected internal set; }
#if SILVERLIGHT
        // Need a wrapper around System.Net.WebHeaderCollection to allow headers in mocks   
#endif
        
        public virtual string Content { get; set; }

#if WindowsPhone 
        public long ContentLength { get; set; }
#elif !SILVERLIGHT
        public override long ContentLength { get; set; }
#elif !WindowsPhone
        public long ContentLength { get; set; }
#endif
        public override string ContentType { get; set; }

        public MockHttpWebRequest(Uri requestUri)
        {
            _requestUri = requestUri;
            Initialize();
        }

        private void Initialize()
        {
            Headers = new System.Net.WebHeaderCollection();
            ExpectHeaders = new WebHeaderCollection();
        }

#if !SILVERLIGHT
        public override WebResponse GetResponse()
        {
            return CreateResponse();
        }
#endif      
        public override void Abort()
        {
            
        }

        private WebResponse CreateResponse()
        {
            var response = new MockHttpWebResponse(_requestUri, ContentType)
            {
                StatusCode = ExpectStatusCode,
                StatusDescription = ExpectStatusDescription,
                Content = Content
            };

            foreach (var key in ExpectHeaders.AllKeys)
            {
                response.MockHeaders.Add(key, ExpectHeaders[key].Value);
            }

            return response;
        }

#if !SILVERLIGHT
        public override Stream GetRequestStream()
        {
            return new MemoryStream();
        }
#endif
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            // [DC]: Mock POSTs never write to the request
            return BeginGetResponse(callback, state);

            /* var result = new WebQueryAsyncResult
                             {
                                 AsyncState = new MemoryStream(),
                                 IsCompleted = true,
                                 CompletedSynchronously = true
                             };*/
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            var response = CreateResponse();
            var result = new WebQueryAsyncResult
                             {
                                 AsyncState = response,
                                 IsCompleted = true,
                                 CompletedSynchronously = true
                             };

            return result;
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            var result = (WebQueryAsyncResult) asyncResult;
            return result.AsyncState as MemoryStream;
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            var result = (WebQueryAsyncResult)asyncResult;
            return result.AsyncState as WebResponse;
        }

        public override System.Net.WebHeaderCollection Headers { get; set; }
        public override string Method { get; set; }

        public override Uri RequestUri
        {
            get { return _requestUri; }
        }

#if !SILVERLIGHT
        public override int Timeout
        {
            get
            {
                return int.MaxValue;
            }
        }
#endif

    }
}