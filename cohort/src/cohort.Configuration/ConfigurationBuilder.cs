using System;
using System.Linq;
using System.Collections.Generic;
using cohort.Models;

namespace cohort.Configuration
{
    public class ConfigurationBuilder
    {
        private static readonly object Sync = new object();

        public static void LoadConfiguration(SiteContext context)
        {
            lock(Sync)
            {
                var settings = Config.Container.Resolve<IConfigRepository>();
                var all = settings.GetAll().ToDictionary(s => s.Key, s => s.Value);

                context.Paging.ResultsPerPage = Int32.Parse(all["cohort.paging.resultsperpage"]);
                context.LocalAuth.Enabled = false;
                context.LocalAuth.AutoRegister = false;
                context.Membership.ActivationDays = int.Parse(all["cohort.membership.activation_days"]);
                context.Membership.PasswordResetDays = int.Parse(all["cohort.membership.password_reset_days"]);
                
                BuildAuthSettings(context, all);
                BuildBillingSettings(context, all);
                BuildEmailSettings(context, all);
            }
        }

        private static void BuildAuthSettings(SiteContext context, IReadOnlyDictionary<string, string> all)
        {
            context.Auth.Username = bool.Parse(all["cohort.auth.username"]);
            context.Auth.SuperUserRole = all["cohort.auth.superuser_role"];
            context.Auth.AdminRole = all["cohort.auth.admin_role"];
            context.Auth.SuperUserPassword = all["cohort.auth.superuser_password"];
            context.Auth.SuperUserEmail = all["cohort.auth.superuser_email"];
            context.Auth.SuperUserApiToken = all["cohort.auth.superuser_api_token"];
        }

        private static void BuildBillingSettings(SiteContext context, IReadOnlyDictionary<string, string> all)
        {
            context.Stripe.Enabled = bool.Parse(all["cohort.stripe.enabled"]);
            context.Stripe.TestMode = bool.Parse(all["cohort.stripe.test_mode"]);
            context.Stripe.TestPublishableKey = all["cohort.stripe.test_publishable_key"];
            context.Stripe.TestSecretKey = all["cohort.stripe.test_secret_key"];
            context.Stripe.CaptureCardOnSignUp = bool.Parse(all["cohort.stripe.capture_card_on_signup"]);
        }

        private static void BuildEmailSettings(SiteContext context, IReadOnlyDictionary<string, string> all)
        {
            context.Email.TestMode = bool.Parse(all["cohort.email.test_mode"]);
            context.Email.TestProvider = all["cohort.email.test_provider"];
            context.Email.TestProviderKey = all["cohort.email.test_provider_key"];
            context.Email.LiveProvider = all["cohort.email.live_provider"];
            context.Email.LiveProviderKey = all["cohort.email.live_provider_key"];

            context.Email.FromAddress = all["cohort.email.from_address"];
            context.Email.ActivationSubject = all["cohort.email.activation_subject"];
            context.Email.ResetSubject = all["cohort.email.reset_subject"];
            context.Email.ContactSubject = all["cohort.email.contact_subject"];
            context.Email.Templates.Add("Activation", all["cohort.email.activation_template"]);
            context.Email.Templates.Add("ResetPassword", all["cohort.email.reset_template"]);
            context.Email.Templates.Add("Contact", all["cohort.email.contact_template"]);
        }
    }
}