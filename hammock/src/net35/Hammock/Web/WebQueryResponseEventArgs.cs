using System;
using System.IO;
using System.Net;

namespace Hammock.Web
{
    public class WebQueryResponseEventArgs : EventArgs
    {
        public WebQueryResponseEventArgs(Stream response)
        {
            Response = response;
        }

        public WebQueryResponseEventArgs(Stream response, Exception exception)
        {
            Response = response;
            Exception = exception;
        }

        public Stream Response { get; set; }
        public Exception Exception { get; set; }
    }
}