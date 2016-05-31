using System;

namespace Hammock.Web
{
    public class WebQueryRequestEventArgs : EventArgs
    {
        public WebQueryRequestEventArgs(string request)
        {
            Request = request;
        }

        public string Request { get; private set; }
    }
}