using FluentMigrator;

namespace linger.Tests.Migrations
{
    [Profile("linger")]
    public class AddRepeatInfo : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("RepeatInfo")
                .WithColumn("ScheduledJobId").AsInt32().Identity().ForeignKey("ScheduledJob", "Id")
                .WithColumn("PeriodFrequency").AsInt32().NotNullable()
                .WithColumn("PeriodQuantifier").AsInt32().NotNullable()
                .WithColumn("Start").AsDateTime().NotNullable()
                .WithColumn("IncludeWeekends").AsBoolean().NotNullable().WithDefaultValue(false)
                ;
        }
    }
}