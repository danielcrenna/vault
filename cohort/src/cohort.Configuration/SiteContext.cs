namespace cohort.Configuration
{
    public class SiteContext
    {
        public SiteContext()
        {
            Auth = new AuthConfig();
            LocalAuth = new LocalAuthConfig();
            Stripe = new StripeConfig();
            Email = new EmailConfig();
            Membership = new MembershipConfig();
            Paging = new PagingConfig();

            ConfigurationBuilder.LoadConfiguration(this);
        }

        /// <summary>
        /// Allows local users to log in automatically. If the user is part of an active directory, their email
        /// information is used to create their account. If this feature is disabled in the future, the users who
        /// logged in locally will need their passwords reset.
        /// </summary>
        public LocalAuthConfig LocalAuth { get; set; }
        public AuthConfig Auth { get; set; }
        public EmailConfig Email { get; set; }
        public StripeConfig Stripe { get; set; }
        public MembershipConfig Membership { get; set; }
        public PagingConfig Paging { get; set; }
    }
}