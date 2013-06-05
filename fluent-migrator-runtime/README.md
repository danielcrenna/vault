#### fluent-migrator-runtime

This small library extends FluentMigrator to allow you to run migrations at runtime, on any migrations-containing assembly,
without requiring executables or other configuration. This means that if you're feeling adventurous, you can run migrations 
on application startup, and never have to think about build scripts for your database. If not, you can still use this
in your own executable process to simplify calling into FluentMigrator to get the job done.

### Using runtime migrations
```csharp
var connectionString = ConfigurationManager.ConnectionStrings["Wherever"].ConnectionString;

// Migrate the currently executing assembly to latest
Migrations.MigrateToLatest(DatabaseType.SqlServer, connectionString);

// Migrate the currently executing assembly to a specific version
Migrations.MigrateTo(DatabaseType.Sqlite, connectionString, 5);

// Migrate a supporting assembly to latest
Migrations.MigrateToLatest(DatabaseType.MySql, connectionString, typeof(MyDomainLibrary).Assembly);

// Migrate a supporting assembly to a specific version
Migrations.MigrateTo(DatabaseType.Oracle, connectionString, 5, typeof(MyDomainLibrary).Assembly);
```
