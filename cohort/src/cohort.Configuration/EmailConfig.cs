using System;
using System.Collections.Generic;
using System.Web.Hosting;

namespace cohort.Configuration
{
    public class EmailConfig
    {
        public string BaseDirectory { get; set; }
        public IDictionary<string, string> Templates { get; private set; }
        public string FromAddress { get; set; }
        public string ActivationSubject { get; set; }
        public string ResetSubject { get; set; }
        public string ContactSubject { get; set; }

        public bool TestMode { get; set; }
        public string TestProvider { get; set; }
        public string TestProviderKey { get; set; }
        public string LiveProvider { get; set; }
        public string LiveProviderKey { get; set; }

        public string Provider
        {
            get { return TestMode ? TestProvider : LiveProvider; }
        }
        public string ProviderKey
        {
            get { return TestMode ? TestProviderKey : LiveProviderKey; }
        }

        public EmailConfig()
        {
            TestMode = true;
            BaseDirectory = HostingEnvironment.MapPath("~/");
            Templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}