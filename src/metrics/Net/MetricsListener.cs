using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using metrics.Util;

namespace metrics.Net
{
    public class MetricsListener : IDisposable
    {
        public const string NotFoundResponse = "<!doctype html><html><body>Resource not found</body></html>";
        public const string PingResponse = "pong";
        private HttpListener _listener;
        private CancellationTokenSource _task;

        private readonly Metrics _metrics;

        public MetricsListener(Metrics metrics)
        {
            _metrics = metrics;
        }
        private static HttpListener InitializeListenerOnPort(int port)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(string.Format("http://localhost:{0}/", port));
            return listener;
        }

        public static void ListenerCallback(IAsyncResult result)
        {

        }

        public void Dispose()
        {
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
            }
        }

        public void Start(int port)
        {
            _listener = _listener ?? InitializeListenerOnPort(port);
            _listener.Start();

            _task = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                while (!_task.IsCancellationRequested)
                {
                    var context = _listener.GetContext();

                    ThreadPool.QueueUserWorkItem(_ => HandleContext(context));
                }
            }, _task.Token);
        }

        private  void HandleContext(HttpListenerContext context)
        {
            var request = context.Request; 
            var response = context.Response;
           // var metrics = new Metrics();
           
 
            // TODO: parse 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8'
            // http://www.singular.co.nz/blog/archive/2008/07/06/finding-preferred-accept-encoding-header-in-csharp.aspx

            var mimeType = request.Headers["Accept"] ?? "application/json";
            
            switch (request.RawUrl)
            {
                case "/static/jquery.js":
                    RespondWithFile(response, "jquery.js");
                    break;
                case "/static/jquery.flot.min.js":
                    RespondWithFile(response, "jquery.flot.min.js");
                    break;
                case "/":
                    if(mimeType.StartsWith("text/html"))
                            RespondWithFile(response, "index.html");
                    else // "application/json"
                            RespondWithNotFound(response);
                    break;
                case "/ping":
                    response.StatusCode = 200;
                    response.StatusDescription = "OK";
                    WriteFinal(PingResponse, response);
                    break;
                case "/metrics":
                    response.StatusCode = 200;
                    response.StatusDescription = "OK";

                    switch(mimeType)
                    {
                        case "text/html":
                            WriteFinal(Serializer.Serialize(_metrics.AllSorted), response);
                            break;
                        default: // "application/json"
                            WriteFinal(Serializer.Serialize(_metrics.AllSorted), response);
                            break;
                    }
                    
                    break;
                default:
                    RespondWithNotFound(response);
                    break;
            }
        }

        private static void RespondWithFile(HttpListenerResponse response, string filename)
        {
            response.StatusCode = 200;
            response.StatusDescription = "OK";
            var file = ReadFromManifestResourceStream("metrics.Net.Static." + filename);
            WriteFinal(file, response);
        }

        private static void RespondWithNotFound(HttpListenerResponse response)
        {
            response.StatusCode = 404;
            response.StatusDescription = "NOT FOUND";
            WriteFinal(NotFoundResponse, response);
        }

        private static void WriteFinal(string responseString, HttpListenerResponse response)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public void Stop()
        {
            if(!_task.IsCancellationRequested)
            {
                _task.Cancel();
            }
            _listener.Stop();
        }

        private static string ReadFromManifestResourceStream(string name)
        {
            var assembly = typeof(MetricsListener).Assembly;

            using (var resourceStream = assembly.GetManifestResourceStream(name))
            {
                using(var sr = new StreamReader(resourceStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
