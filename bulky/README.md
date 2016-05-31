bulky
=====

```powershell
PM> Install-Package bulky
```

Bulky is an idiomatic bulk insert interface for databases. It gives you the ability to use the
native bulk copy mechanism of your database simply, using plain C# objects and an extension method on `IDbConnection`, much like how [Dapper](https://github.com/SamSaffron/dapper-dot-net) functions.

### Why would I use this?
- You want simple, high speed bulk inserts of large collections of objects
- You need support for multiple databases (SQL Server, MySQL, and SQLite are supported today)
- It works great with [tophat](https://github.com/danielcrenna/webstack/tree/master/tophat)

### Usage

The hands-free usage is simple:

```csharp
using bulky;

// Get your objects from somewhere
IEnumerable<User> users = ReallyLargeCollectionOfUsers();

// Get your connection from somewhere else
IDbConnection connection = GetMyDatabaseConnectionFromSomewhere();

// Profit!
connection.BulkCopy(users);
```

Behind the scenes, Bulky is using the provided bulk copy implementation.
This is how you can change the underlying strategy:

```csharp
using bulky;

// Change to MySQL multi-value inserts
Bulky.BulkCopier = new MySqlBulkCopy();

// Change to SQLite's transactional flush
Bulky.BulkCopier = new SqliteBulkCopy();

// Change to SQL Server's SqlBulkCopy (the default)
Bulky.BulkCopier = new SqlServerBulkCopy();
```

### How does bulky map objects to database columns?

Under the hood, bulky relies on a [TableDescriptor](http://github.com/danielcrenna/TableDescriptor) definition
of a class in order to map it to database columns. By default, it uses TableDescriptor's built-in `SimpleDescriptor`
to perform that mapping, but you can always pass in any implementation of `Descriptor`. The default conventions
are simple enough, but if you have more advanced mapping needs you'll want to look at that project directly.
For example, this `User` object will map to the database below:

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
}
```

```sql
CREATE TABLE [dbo].[User]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Email] [varchar](255) NOT NULL, 
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
)
```