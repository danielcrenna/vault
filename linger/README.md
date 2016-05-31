### linger
A priority job queuing subsystem, inspired by delayed_job

#### What is this?
Linger allows you to run background tasks simply, while providing some logging and retry facilities.
It was designed to solve the problem of performing background work in web applications (i.e. when
sending an email, running a monthly process to bill customers, etc.). 

Currently, the only real option for background queuing in .NET web apps is to roll your own or use Quartz.NET.
I wanted a lightweight, table-based queue for background tasks that survived app pool restarts if I chose
to hook into the ASP.NET worker process, and the flexibility to move it to a dedicated service or worker
role for scale. Linger runs on a single worker process but can parallelize tasks. You don't have to run
Linger in ASP.NET, but that's my main use case.

#### Caveats
- It's up to you to make your jobs resilient and transactional if necessary; jobs can fail at any time
- The serialized job must have its dependencies available when unpacked; no MSIL stuffing here

#### Installation

Linger doesn't manage your database or assume anything about your backend. All relevant information is
stored in a backing store. Currently, the only available storage is through an ADO.NET database connection,
supporing SQLite, MySQL, and SQL Server.

To install Linger, you must first specify a backend function:
```csharp

// Use a database backend, with the SQLite dialect; the unit of work is any method providing an IDbConnection
var backend = new DatabaseJobRepository(DatabaseDialect.Sqlite, () => UnitOfWork.Current);

// Optionally, if you aren't maintaining the job table yourself, you can issue table creation commands with a helper
backend.InstallSchema();

// Set the backend before starting the process
Linger.Backend = backend;

// For clients that will be performing work in addition to queuing, you must start and stop the worker thread
Linger.StartWorker();
```

#### Defining a job
A job can be any C# class that conforms to the `Performs` contract, it does not have to actually implement anything.

```csharp
public interface Performs
{
	bool Perform();
}
```

The contract asks for a parameterless method to perform the work, and to return a `bool` indicating whether the
attempt was successful.

The following is a valid job:

```csharp
using System;

[Serializable]
public class HelloWorldJob
{
    public bool Perform()
    {
        Console.WriteLine("Hello, world!");
        return true;
    }
}
```

#### Queuing a job

When you want to send the job to the queuing system, you can do so directly, or using extension methods.

```csharp

// Queue the job directly with an explicit job type
Linger.QueueForExecution(new HelloWorldJob())

// Use an extension method on an explicit job type
new HelloWorldJob().PerformAsync();

// Use an extension method on a delegate code block
Func<bool> hello = ()=>
{
    Console.WriteLine("Hello, world!");
    return true;
};
hello.PerformAsync();
```

#### Can I use this in my ASP.NET application?
Yes. Linger will automatically register itself with ASP.NET such that it _may_ allow your task to complete
before the app pool is recycled. You should still protect your tasks, especially those that update the
database, by expecting failure at any time. You can use the special `Halt` event hook, that will execute
when ASP.NET is about to recycle the app pool. This should help you write more resilient tasks. I recommend 
creating a dedicated Windows service to run your linger tasks, but that's not always feasible or expedient.

_Check [this article](http://haacked.com/archive/2011/10/16/the-dangers-of-implementing-recurring-background-tasks-in-asp-net.aspx) for more info._


TODO
- MySQL DDL
- Full documentation
- Have our own task to periodically unlock 'hanging' tasks
- Alternative backends (Redis, etc.)
- Front-end UI dashboard