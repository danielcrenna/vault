using FluentMigrator;

namespace linger.Tests.Migrations
{
    [Profile("linger")]
    public class AddBatches : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Batch")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("StartedAt").AsDateTime().Nullable()
                .WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(0)
                ;

            Create.Table("BatchJob")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("BatchId").AsInt32().ForeignKey("Batch", "Id")
                .WithColumn("ScheduledJobId").AsInt32().ForeignKey("ScheduledJob", "Id")
                ;
        }
    }
}