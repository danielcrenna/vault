using FluentMigrator;

namespace cohort.Migrations
{
    [Migration(3)]
    public class LoggingAndErrors : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Error")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Message").AsString().NotNullable()
                .WithColumn("StackTrace").AsString().Nullable()
                .WithColumn("User").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentDateTime);

            Create.Table("Email")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("From").AsString().NotNullable()
                .WithColumn("ReplyTo").AsString().Nullable()
                .WithColumn("To").AsString().NotNullable()
                .WithColumn("CC").AsString().Nullable()
                .WithColumn("BCC").AsString().Nullable()
                .WithColumn("Subject").AsString().NotNullable()
                .WithColumn("TextBody").AsAnsiString().Nullable()
                .WithColumn("HtmlBody").AsAnsiString().Nullable()
                .WithColumn("DeliveredAt").AsDateTime().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            Create.Table("Log")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Level").AsString().NotNullable()
                .WithColumn("Message").AsString().NotNullable()
                .WithColumn("StackTrace").AsString().Nullable()
                .WithColumn("User").AsString().Nullable()
                .WithColumn("IPAddress").AsString().Nullable()
                .WithColumn("Path").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().WithDefault(SystemMethods.CurrentDateTime);

            Create.Table("ContactList")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Email").AsString().NotNullable();
        }
    }
}