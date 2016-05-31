using System.Diagnostics;
using System.IO;
using System.Net;
using NUnit.Framework;
using metrics.Net;

namespace metrics.Tests.Net
{
    [TestFixture]
    public class MetricsListenerTests
    {
        private MetricsListener _listener;
        private const int Port = 9898;
        Metrics _metrics=new Metrics();


        [TestFixtureSetUp]
        public void SetUp()
        {
            _listener = new MetricsListener(_metrics);
            _listener.Start(Port);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _listener.Stop();
        }

        [Test]
        public void Can_stop_gracefully()
        {
            Can_respond_to_ping_request();
            _listener.Stop();
            _listener.Stop();
        }

        [Test]
        public void Can_respond_with_not_found_with_body_when_path_is_not_found()
        {
            var content = GetResponseForRequest("http://localhost:" + Port + "/unknown");

            Assert.AreEqual("<!doctype html><html><body>Resource not found</body></html>", content);
        }
        
        [Test]
        public void Can_respond_to_ping_request()
        {
            var content = GetResponseForRequest("http://localhost:" + Port + "/ping");
            
            Assert.AreEqual("pong", content);
        }

        [Test]
        public void Can_respond_to_metrics_request_when_metrics_are_registered()
        {
            _metrics.Clear();

            var counter = _metrics.Counter(typeof(MetricsListenerTests), "counter");
            
            counter.Increment();

            var content = GetResponseForRequest("http://localhost:" + Port + "/metrics");
            const string expected = @"[{""name"":""counter"",""metric"":{""count"":1}}]";
            Assert.AreEqual(expected, content);
        }

        [Test]
        public void Can_request_base_url_in_html()
        {
            var content = GetResponseForRequest("http://localhost:" + Port + "/", "text/html");
            
            Trace.WriteLine(content);
        }
        
        [Test]
        public void Can_respond_to_metrics_request_when_no_metrics_are_registered()
        {
            _metrics.Clear();

            var content = GetResponseForRequest("http://localhost:" + Port + "/metrics");

            Assert.AreEqual("[]", content);
        }

        private static string GetResponseForRequest(string url, string accept = "application/json")
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Accept = accept;

                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    return result;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                {
                    using (var esr = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        return esr.ReadToEnd();
                    }
                }

                return null;
            }
        }
    }
}
