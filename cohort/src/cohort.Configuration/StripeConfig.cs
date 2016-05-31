namespace cohort
{
    public class StripeConfig
    {
        public bool Enabled { get; set; }
        public bool CaptureCardOnSignUp { get; set; }
        
        public string PublishableKey
        {
            get { return TestMode ? TestPublishableKey : LivePublishableKey; }
        }
        public string SecretKey
        {
            get { return TestMode ? TestSecretKey : LiveSecretKey; }
        }

        public bool TestMode { get; set; }
        public string TestPublishableKey { get; set; }
        public string TestSecretKey { get; set; }
        public string LivePublishableKey { get; set; }
        public string LiveSecretKey { get; set; }

        public StripeConfig()
        {
            TestMode = true;
        }
    }
}