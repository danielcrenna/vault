# tophat

```
PM> Install-Package tophat
```

### Introduction
Tophat gives you a unit of work to perform data operations against, independent of your database.
Any database that provides support for `IDbConnection` works in tophat. Most ORMs today provide
some kind of session scoping (NHibernate uses Session, Linq-To-SQL uses DataContext, etc.) but
for many use cases, ORMs are unnecessary. New micro-ORM layers like Dapper and PetaPoco simply extend
the `IDbConnection` interface. Tophat provides scoping the same way.

### Features

* `UnitOfWork` or `DataContext` for your `IDbConnection`
* Simple interface for plugging in your own connection factory
* Common scopes provided, or pass in a custom scope function
* Why, that's a lovely top hat you're wearing! (works nicely with Dapper) 

### Installing a connection factory (for unit of work support)

```csharp
// One database connection per web request
var connectionString = "Data Source=(local);Initial Catalog=MyDatabase;Integrated Security=true";
Database.Install<SqlServerConnectionFactory>(connectionString, ConnectionScope.ByRequest);
```

### Using the unit of work
```csharp
using tophat;
using Dapper;

public class MyRepository
{
    public IEnumerable<Foo> GetFooz()
	{
		// Automatic handling of connection scope
		IDbConnection db = UnitOfWork.Current;

		// Regular Dapper operations against an IDbConnection
		return db.Query<Fooz>("SELECT * FROM Fooz");
	}
}
```

### Using the DataContext directly
```csharp
using tophat;
using Dapper;

public class MyRepository
{
    public IEnumerable<Foo> GetFooz()
	{
	     // Manual handling of connection scope
		 var cs = "Data Source=(local);Initial Catalog=MyDatabase;Integrated Security=true";
         using (var db = new SqlServerDataContext(cs))
         {
             // Regular Dapper operations against an IDbConnection
		     return db.Query<Foo>("SELECT * FROM Fooz");
         }		
	}
}
```

### Implementing support for a custom database (in this case, SQLite3)
```csharp
using tophat;
using System.Data;
using System.Data.SQLite;

public class SqliteConnectionFactory : ConnectionFactory
{
    public override IDbConnection CreateConnection()
    {
        return new SQLiteConnection(ConnectionString);
    }
}

public class SqliteDataContext : DataContext<SqliteConnectionFactory>
{
    public SqliteDataContext(string connectionString) : base(connectionString)
    {

    }
}
```

### Intercepting the furnished connection (i.e. when you want to wrap it)
```csharp
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

// ...

UnitOfWork.Adapter = connection => new ProfiledDbConnection((DbConnection) connection, MiniProfiler.Current); 
```

### Managing an external connection (i.e. when you want to borrow a connection from another layer)
```csharp
Database.Install(connectionString, (cs) => SomeOtherFactory.GetConnection(cs), ConnectionScope.ByRequest);
```