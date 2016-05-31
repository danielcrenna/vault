using System;
using System.Data;
using Dapper;

namespace linger
{
    internal static class Schema
    {
        public static void Install(DatabaseDialect dialect, IDbConnection db)
        {
            string sql;
            switch(dialect)
            {
                case DatabaseDialect.SqlServer:
                    sql = "IF NOT EXISTS (SELECT [name] FROM sys.tables WHERE [name] = 'ScheduledJob')\n" +
                          "CREATE TABLE [dbo].[ScheduledJob]\n" +
                          "(\n" +
                          "    [Id] INT NOT NULL IDENTITY(1,1), \n" +
                          "    [Name] NVARCHAR(255), \n" +
                          "    [Priority] INT NOT NULL CONSTRAINT [DF_ScheduledJob_Priority] DEFAULT 0, \n" +
                          "    [Attempts] INT NOT NULL CONSTRAINT [DF_ScheduledJob_Attempts] DEFAULT 0, \n" +
                          "    [Handler] VARBINARY(8000) NOT NULL, \n" +
                          "    [LastError] NVARCHAR(255), \n" +
                          "    [RunAt] DATETIME, \n" +
                          "    [FailedAt] DATETIME, \n" +
                          "    [SucceededAt] DATETIME, \n" +
                          "    [LockedAt] DATETIME, \n" +
                          "    [LockedBy] NVARCHAR(255), \n" +
                          "    [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_ScheduledJob_CreatedAt] DEFAULT GETDATE(), \n" +
                          "    [UpdatedAt] DATETIME, \n" +
                          "    CONSTRAINT [PK_ScheduledJob] PRIMARY KEY ([Id])\n" +
                          ");\n";

                    sql += "IF NOT EXISTS (SELECT [name] FROM sys.tables WHERE [name] = 'Batch' )\n" + 
                           "CREATE TABLE [dbo].[Batch]\n" +
                           "(\n" +
                           "    [Id] INT NOT NULL IDENTITY(1,1), \n" +
                           "    [Name] NVARCHAR(255) NOT NULL, \n" +
                           "    [StartedAt] DATETIME, \n" +
                           "    [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_Batch_CreatedAt] DEFAULT GETDATE(), \n" +
                           "    CONSTRAINT [PK_Batch] PRIMARY KEY ([Id])\n" +
                           ");\n";

                    sql += "IF NOT EXISTS (SELECT [name] FROM sys.tables WHERE [name] = 'BatchJob')\n" + 
                           "CREATE TABLE [dbo].[BatchJob]\n" +
                           "(\n" +
                           "    [Id] INT NOT NULL IDENTITY(1,1), \n" +
                           "    [BatchId] INT NOT NULL, \n" +
                           "    [ScheduledJobId] INT NOT NULL, \n" +
                           "    CONSTRAINT [PK_BatchJob] PRIMARY KEY ([Id])\n" +
                           ");\n";

                    sql += "IF NOT EXISTS (SELECT [name] FROM sys.objects WHERE [name] = 'FK_BatchJob_BatchId_Batch_Id')\n" +
                           "ALTER TABLE [dbo].[BatchJob] ADD CONSTRAINT [FK_BatchJob_BatchId_Batch_Id] FOREIGN KEY ([BatchId]) REFERENCES [dbo].[Batch] ([Id]);\n";
                    
                    sql += "IF NOT EXISTS (SELECT [name] FROM sys.objects WHERE [name] = 'FK_BatchJob_ScheduledJobId_ScheduledJob_Id')\n" + 
                           "ALTER TABLE [dbo].[BatchJob] ADD CONSTRAINT [FK_BatchJob_ScheduledJobId_ScheduledJob_Id] FOREIGN KEY ([ScheduledJobId]) REFERENCES [dbo].[ScheduledJob] ([Id]);\n";

                    sql += "IF NOT EXISTS (SELECT [name] FROM sys.tables WHERE [name] = 'RepeatInfo')\n" + 
                           "CREATE TABLE [dbo].[RepeatInfo] \n" +
                           "(\n" +
                           "    [ScheduledJobId] INT NOT NULL IDENTITY(1,1), \n" +
                           "    [PeriodFrequency] INT NOT NULL, \n" +
                           "    [PeriodQuantifier] INT NOT NULL, \n" +
                           "    [Start] DATETIME NOT NULL, \n" +
                           "    [IncludeWeekends] BIT NOT NULL CONSTRAINT [DF_RepeatInfo_IncludeWeekends] DEFAULT 0\n" +
                           ");\n";

                    sql += "IF NOT EXISTS ( SELECT [name] FROM sys.objects WHERE [name] = 'FK_RepeatInfo_ScheduledJobId_ScheduledJob_Id')\n" +
                           "ALTER TABLE [dbo].[RepeatInfo] ADD CONSTRAINT [FK_RepeatInfo_ScheduledJobId_ScheduledJob_Id] FOREIGN KEY ([ScheduledJobId]) REFERENCES [dbo].[ScheduledJob] ([Id]);\n";
                    break;

                case DatabaseDialect.MySql:
                    throw new NotImplementedException();
                case DatabaseDialect.Sqlite:
                    sql = "CREATE TABLE IF NOT EXISTS 'ScheduledJob' " +
                          "(" +
                          "    'Id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                          "    'Name' TEXT, " +
                          "    'Priority' INTEGER NOT NULL DEFAULT 0, " +
                          "    'Attempts' INTEGER NOT NULL DEFAULT 0, " +
                          "    'Handler' BLOB NOT NULL, " +
                          "    'LastError' TEXT, " +
                          "    'RunAt' DATETIME, " +
                          "    'FailedAt' DATETIME, " +
                          "    'SucceededAt' DATETIME, " +
                          "    'LockedAt' DATETIME, " +
                          "    'LockedBy' TEXT, " +
                          "    'CreatedAt' DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, " +
                          "    'UpdatedAt' DATETIME " +
                          ");";

                    sql += "CREATE TABLE IF NOT EXISTS 'Batch' (" +
                           "    'Id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                           "    'Name' TEXT NOT NULL, " +
                           "    'StartedAt' DATETIME," +
                           "    'CreatedAt' DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP " +
                           ");";

                    sql += "CREATE TABLE IF NOT EXISTS 'BatchJob' " +
                           "(" +
                           "    'Id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                           "    'BatchId' INTEGER NOT NULL, " +
                           "    'ScheduledJobId' INTEGER NOT NULL" +
                           ");";

                    sql += "CREATE TABLE IF NOT EXISTS 'RepeatInfo' " +
                           "(" +
                           "    'ScheduledJobId' INTEGER NOT NULL, " +
                           "    'PeriodFrequency' INTEGER NOT NULL, " +
                           "    'PeriodQuantifier' INTEGER NOT NULL, " +
                           "    'Start' DATETIME NOT NULL, " +
                           "    'IncludeWeekends' INTEGER NOT NULL DEFAULT 0" +
                           ")";
                    break;
                default:
                    throw new NotSupportedException();
            }

            db.Execute(sql);
        }
    }
}