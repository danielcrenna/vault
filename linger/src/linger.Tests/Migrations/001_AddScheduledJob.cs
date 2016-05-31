using FluentMigrator;

namespace linger.Tests.Migrations
{
    [Profile("linger")]
    public class AddScheduledJob : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("ScheduledJob")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("Attempts").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("Handler").AsBinary().NotNullable()
                .WithColumn("LastError").AsString().Nullable()
                .WithColumn("RunAt").AsDateTime().Nullable()
                .WithColumn("FailedAt").AsDateTime().Nullable()
                .WithColumn("SucceededAt").AsDateTime().Nullable()
                .WithColumn("LockedAt").AsDateTime().Nullable()
                .WithColumn("LockedBy").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsDateTime().Nullable();
        }
    }
}