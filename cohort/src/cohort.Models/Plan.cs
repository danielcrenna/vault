using System.Collections.Generic;
using Stripe;
using TableDescriptor;

namespace cohort.Models
{
    public class Plan : StripePlan 
    {
        public string Tag { get; set; }
        public string Description { get; set; }

        [Transient]
        public List<string> Features { get; set; }

        public Plan()
        {
            Features = new List<string>();
        }
    }

    //var stripe = Create.Table("Plan")
    //            .WithColumn("Id").AsString().NotNullable()
    //            .WithColumn("Name").AsString().NotNullable()
    //            .WithColumn("Currency").AsString(3).NotNullable()
    //            .WithColumn("LiveMode").AsBoolean()
    //            .WithColumn("Amount").AsInt16()
    //            .WithColumn("Interval").AsInt16()
    //            .WithColumn("IntervalCount").AsInt16()
    //            .WithColumn("TrialPeriodDays").AsInt16().Nullable()
    //            .WithColumn("UpdatedAt").AsDateTime();

    //        stripe
    //            .WithColumn("Tag").AsString().Nullable()
    //            .WithColumn("Description").AsString().Nullable();

    //        Create.Table("PlanFeature")
    //            .WithColumn("PlanId").AsString().ForeignKey("Plan", "Id")
    //            .WithColumn("Feature").AsString();
}