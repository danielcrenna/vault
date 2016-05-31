using FluentMigrator;

namespace cohort.Migrations
{
    [Migration(4)]
    public class BillingAndDunning : AutoReversingMigration
    {
        public override void Up()
        {
            // Directly mapped to Stripe
            Create.Table("StripeEvent")
                .WithColumn("Id").AsString().NotNullable().PrimaryKey()
                .WithColumn("Type").AsString().NotNullable()
                .WithColumn("Data").AsString().NotNullable()
                .WithColumn("LiveMode").AsBoolean().NotNullable()
                .WithColumn("Created").AsDateTime().NotNullable();

            // Identical to stripe plan with two additional fields for the UI (tag and description)
            Create.Table("Plan")
                .WithColumn("Id").AsString().NotNullable().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Currency").AsString(3).NotNullable()
                .WithColumn("LiveMode").AsBoolean().Nullable()
                .WithColumn("AmountInCents").AsInt16().Nullable()
                .WithColumn("Interval").AsString()
                .WithColumn("IntervalCount").AsInt16()
                .WithColumn("TrialPeriodDays").AsInt16().Nullable()
                .WithColumn("Tag").AsString().Nullable()
                .WithColumn("Description").AsString().Nullable();
            
            Create.Table("PlanFeature")
                .WithColumn("PlanId").AsString().ForeignKey("Plan", "Id")
                .WithColumn("Feature").AsString();
        }
    }
}