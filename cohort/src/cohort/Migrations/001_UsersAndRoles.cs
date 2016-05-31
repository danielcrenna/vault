using FluentMigrator;

namespace cohort.Migrations
{
    [Migration(1)]
    public class UsersAndRoles : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("User")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey().Indexed()
                .WithColumn("Email").AsAnsiString().NotNullable().Indexed()
                .WithColumn("Username").AsAnsiString().Nullable().Indexed()
                .WithColumn("Password").AsFixedLengthString(70).NotNullable()
                .WithColumn("ReferrerUrl").AsAnsiString().Nullable()
                .WithColumn("LandingPageUrl").AsAnsiString().Nullable()
                .WithColumn("Culture").AsString(8).NotNullable()
                .WithColumn("IPAddress").AsString(46).Nullable()
                .WithColumn("IsActivated").AsBoolean().WithDefaultValue(false)
                .WithColumn("JoinedOn").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("SignedInOn").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("SignedOutOn").AsDateTime().Nullable()
                .EffectiveDates();

            Create.Table("Role")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().Indexed()
                .WithColumn("Description").AsAnsiString().Unique();
            
            Create.Table("UserRole")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().Indexed()
                .WithColumn("UserId").AsInt32().ForeignKey("User", "Id")
                .WithColumn("RoleId").AsInt32().ForeignKey("Role", "Id");
        }
    }
}