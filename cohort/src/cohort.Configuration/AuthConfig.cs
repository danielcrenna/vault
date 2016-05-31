namespace cohort.Configuration
{
    public class AuthConfig
    {
        public bool Username { get; set; }
        public string AdminRole { get; set; }
        public string SuperUserRole { get; set; }
        public string SuperUserPassword { get; set; }
        public string SuperUserEmail { get; set; }
        public string SuperUserApiToken { get; set; }
    }
}