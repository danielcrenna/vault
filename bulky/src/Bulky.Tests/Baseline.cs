using FluentMigrator;

namespace bulky.Tests
{
    [Migration(1)]
    public class Baseline : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("User")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Email").AsAnsiString();

            Create.Table("UserRole")
                .WithColumn("UserId").AsInt32().PrimaryKey()
                .WithColumn("RoleId").AsInt32().PrimaryKey();
        }
    }
}