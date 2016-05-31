using FluentMigrator;

namespace cohort.Migrations
{
    [Migration(5)]
    public class EverythingElse : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Activation")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().Indexed()
                .WithColumn("Hash").AsFixedLengthAnsiString(32).NotNullable().Indexed()
                .WithColumn("Email").AsAnsiString().NotNullable().Indexed();

            Create.Table("Token")
                .WithColumn("UserId").AsInt32().ForeignKey("User", "Id").Indexed()
                .WithColumn("Value").AsFixedLengthString(36).NotNullable().Indexed()
                .EffectiveDates();

            Create.Table("BrokenLink")
                .WithColumn("Path").AsString(2500).PrimaryKey().Indexed()
                .WithColumn("Method").AsString(7).NotNullable()
                .WithColumn("Referer").AsString(2500).Nullable()
                .WithColumn("Count").AsInt32().NotNullable().WithDefaultValue(1).Indexed()
                .WithColumn("LastOccurrence").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Create.Table("Profile")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey().Indexed()
                .WithColumn("Key").AsAnsiString().NotNullable().Unique()
                .WithColumn("Theme").AsString().NotNullable().WithDefaultValue("Default");
        }
    }
}
