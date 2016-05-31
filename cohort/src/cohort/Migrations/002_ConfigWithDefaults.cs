using System;
using FluentMigrator;
using FluentMigrator.Builders.Insert;

namespace cohort.Migrations
{
    [Migration(2)]
    public class ConfigWithDefaults : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Config")
                .WithColumn("Key").AsString().NotNullable().PrimaryKey().Indexed()
                .WithColumn("Value").AsString().NotNullable();

            Insert.IntoTable("Config")
                .SetAuthVariables().SetBilling().SetEmail().SetRouting()
                .Row(new {Key = "cohort.paging.resultsperpage", Value = "25"})
                .Row(new {Key = "cohort.membership.activation_days", Value = "3"})
                .Row(new {Key = "cohort.membership.password_reset_days", Value = "3"})
                .Row(new {Key = "cohort.pagespeed.enabled", Value = "true"})
                ;
        }
    }

    internal static class ConfigExtensions
    {
        public static IInsertDataSyntax SetAuthVariables(this IInsertDataSyntax root)
        {
            return root
                .Row(new { Key = "cohort.auth.admin_role", Value = "admin" })
                .Row(new { Key = "cohort.auth.superuser_role", Value = "super" })
                .Row(new { Key = "cohort.auth.superuser_email", Value = "daniel.crenna@gmail.com" })
                .Row(new { Key = "cohort.auth.superuser_password", Value = "rosebud" })
                .Row(new { Key = "cohort.auth.superuser_api_token", Value = Guid.NewGuid().ToString() })
                .Row(new { Key = "cohort.auth.username", Value = "false" });
        }

        public static IInsertDataSyntax SetRouting(this IInsertDataSyntax root)
        {
            return root
                .Row(new { Key = "cohort.routing.signup", Value = "signup" })
                .Row(new { Key = "cohort.routing.signin", Value = "signin" })
                .Row(new { Key = "cohort.routing.404", Value = "404" })
                .Row(new { Key = "cohort.routing.500", Value = "500" });
        }

        public static IInsertDataSyntax SetEmail(this IInsertDataSyntax root)
        {
            return root
                .Row(new { Key = "cohort.email.test_mode", Value = "true" })
                .Row(new { Key = "cohort.email.test_provider", Value = "directory" })
                .Row(new { Key = "cohort.email.test_provider_key", Value = "~/Pickup" })
                .Row(new { Key = "cohort.email.live_provider", Value = "postmark" })
                .Row(new { Key = "cohort.email.live_provider_key", Value = "6c678123-90b2-40e0-a78f-13a434dc67fe" })
                .Row(new { Key = "cohort.email.from_address", Value = "daniel.crenna@gmail.com" })
                .Row(new { Key = "cohort.email.activation_subject", Value = "[cohort] Activate your account" })
                .Row(new { Key = "cohort.email.activation_template", Value = "/Views/Email/Activation.liquid" })
                .Row(new { Key = "cohort.email.reset_subject", Value = "[cohort] Reset your password" })
                .Row(new { Key = "cohort.email.reset_template", Value = "/Views/Email/ResetPassword.liquid" })
                .Row(new { Key = "cohort.email.contact_subject", Value = "[cohort] Contact email received" })
                .Row(new { Key = "cohort.email.contact_template", Value = "/Views/Email/Contact.liquid" });
        }

        public static IInsertDataSyntax SetBilling(this IInsertDataSyntax root)
        {
            return root
                .Row(new { Key = "cohort.stripe.enabled", Value = "true" })
                .Row(new { Key = "cohort.stripe.test_mode", Value = "true" })
                .Row(new { Key = "cohort.stripe.test_publishable_key", Value = "pk_0I1ZVgGP3mF1JfBAWfsGeM5XdGgIU" })
                .Row(new { Key = "cohort.stripe.test_secret_key", Value = "sk_0I1Z2wWDHLUfrBhUk7kdA4vbNig6M" })
                .Row(new { Key = "cohort.stripe.live_publishable_key", Value = "[None]" })
                .Row(new { Key = "cohort.stripe.live_secret_key", Value = "[None]" })
                .Row(new { Key = "cohort.stripe.capture_card_on_signup", Value = "true" });
        }
    }
}

                    