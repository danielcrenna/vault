using System;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using cohort.Api.SelfHost;

namespace cohort.Tests.Api
{
    public class ApiFixture
    {
        protected HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    throw new Exception("You need to run Visual Studio in administrator mode to test APIs.");
                }
                return _client;
            }
        }
        private HttpClient _client;
        protected string BaseAddress { get; private set; }

        [TestFixtureSetUp]
        public void SetUp()
        {
            var host = new ApiHost();
            host.Initialize();
            _client = host.CreateTestClient();
            BaseAddress = host.BaseAddress;
        }
    }

    public static class HttpStatusCodeExtensions
    {
        public static void Is(this HttpResponseMessage response, HttpStatusCode code)
        {
            Assert.AreEqual(code, response.StatusCode);
        }
    }
}