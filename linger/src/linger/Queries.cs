using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using Dapper;
using Dates;

namespace linger
{
    internal static class Queries
    {
        public static ScheduledJob GetJob(IDbConnection db, int id)
        {
            const string sql = "SELECT * FROM ScheduledJob WHERE Id = @Id";
            return db.Query<ScheduledJob>(sql, new { Id = id }).FirstOrDefault();
        }

        public static void DeleteJob(IDbConnection db, ScheduledJob job)
        {
            const string sql = "DELETE FROM ScheduledJob WHERE Id = @Id; DELETE FROM RepeatInfo WHERE ScheduledJobId = @Id;";
            db.Execute(sql, job);
        }

        public static void InsertJob(IDbConnection db, DatabaseDialect dialect, ScheduledJob job)
        {
            var sql = "INSERT INTO ScheduledJob " +
                      "(Priority, Attempts, Handler, RunAt) " +
                      "VALUES (@Priority, @Attempts, @Handler, @RunAt); ";

            sql = AddInsertIdentity(dialect, sql);
            job.Id = db.Execute(sql, job);
            job.CreatedAt = db.Query<DateTime>("SELECT CreatedAt FROM ScheduledJob WHERE Id = @Id", new {job.Id}).Single();
        }

        private static string AddInsertIdentity(DatabaseDialect dialect, string sql)
        {
            switch (dialect)
            {
                case DatabaseDialect.SqlServer:
                    sql += "SELECT SCOPE_IDENTITY() AS [Id]";
                    break;
                case DatabaseDialect.MySql:
                    sql += "SELECT LAST_INSERT_ID() AS `Id`";
                    break;
                case DatabaseDialect.Sqlite:
                    sql += "SELECT LAST_INSERT_ROWID() AS \"Id\"";
                    break;
                default:
                    throw new NotSupportedException();
            }
            return sql;
        }

        public static void UpdateJob(IDbConnection db, ScheduledJob job)
        {
            const string sql = "UPDATE ScheduledJob SET " +
                               "Priority = @Priority, Attempts = @Attempts, LastError = @LastError, RunAt = @RunAt, " +
                               "FailedAt = @FailedAt, SucceededAt = @SucceededAt, LockedAt = @LockedAt, " +
                               "LockedBy = @LockedBy, UpdatedAt = @UpdatedAt " +
                               "WHERE Id = @Id";

            db.Execute(sql, job);
        }

        public static IEnumerable<ScheduledJob> GetNextAvailable(IDbConnection db, DatabaseDialect dialect, int count)
        {
            // - None failed or succeeded, none locked, none in batches, RunAt sorted, Priority sorted
            string sql;
            switch(dialect)
            {
                case DatabaseDialect.SqlServer:
                    sql =
                        "SELECT TOP " + count + " * FROM [ScheduledJob] j " +
                        "WHERE NOT EXISTS (SELECT 1 FROM BatchJob WHERE ScheduledJobId = j.Id)" +
                        "AND [LockedAt] IS NULL AND [FailedAt] IS NULL AND [SucceededAt] IS NULL " +
                        "AND [RunAt] IS NULL OR GETDATE() >= [RunAt] " +
                        "ORDER BY [Priority], [RunAt] ASC";
                    return db.Query<ScheduledJob>(sql).ToList();
                case DatabaseDialect.MySql:
                    sql =
                        "SELECT * FROM ScheduledJob j " +
                        "WHERE NOT EXISTS (SELECT 1 FROM BatchJob WHERE ScheduledJobId = j.Id)" +
                        "AND LockedAt IS NULL AND FailedAt IS NULL AND SucceededAt IS NULL " +
                        "AND RunAt IS NULL OR NOW() >= RunAt" +
                        "ORDER BY Priority, RunAt ASC " +
                        "LIMIT @Count";
                    return db.Query<ScheduledJob>(sql, new { Count = count }).ToList();
                case DatabaseDialect.Sqlite:
                    sql = 
                        "SELECT * FROM ScheduledJob j " +
                        "WHERE NOT EXISTS (SELECT 1 FROM BatchJob WHERE ScheduledJobId = j.Id) " +
                        "AND LockedAt IS NULL AND FailedAt IS NULL AND SucceededAt IS NULL " +
                        "AND RunAt IS NULL OR CURRENT_TIMESTAMP >= RunAt " +
                        "ORDER BY Priority, RunAt ASC " +
                        "LIMIT @Count";
                    return db.Query<ScheduledJob>(sql, new { Count = count }).ToList();
                default:
                    throw new ArgumentOutOfRangeException("dialect");
            }
        }

        public static IEnumerable<ScheduledJob> GetAll(IDbConnection db)
        {
            return db.Query<ScheduledJob>("SELECT * FROM ScheduledJob j WHERE NOT EXISTS (SELECT 1 FROM BatchJob WHERE ScheduledJobId = j.Id)");
        }

        public static void LockJobs(IDbConnection db, IList<ScheduledJob> jobs)
        {
            const string sql = "UPDATE ScheduledJob SET LockedAt = @Now, LockedBy = @User WHERE Id IN @Ids";
            var now = DateTime.Now;
            var identity = WindowsIdentity.GetCurrent();
            var user = identity == null ? "Unknown" : identity.Name;
            db.Execute(sql, new
            {
                Now = now,
                Ids = jobs.Select(j => j.Id),
                User = user
            });
            foreach (var job in jobs)
            {
                job.LockedAt = now;
                job.LockedBy = user;
            }   
        }

        public static void InsertBatch(IDbConnection db, DatabaseDialect dialect, Batch batch)
        {
            var sql = "INSERT INTO Batch (Name) VALUES (@Name);";
            sql = AddInsertIdentity(dialect, sql);
            batch.Id = db.Execute(sql, batch);
            batch.CreatedAt = db.Query<DateTime>("SELECT CreatedAt FROM Batch WHERE Id = @Id", new { batch.Id }).Single();
        }

        public static void AddToBatch(IDbConnection db, DatabaseDialect dialect, Batch batch, ScheduledJob job)
        {
            const string sql = "INSERT INTO BatchJob (BatchId, ScheduledJobId) VALUES (@BatchId, @ScheduledJobId)";
            db.Execute(sql, new { BatchId = batch.Id, ScheduledJobId = job.Id });
        }

        public static RepeatInfo GetRepeatInfo(IDbConnection db, ScheduledJob job)
        {
            var result = db.Query("SELECT * FROM RepeatInfo WHERE ScheduledJobId = @Id", job).SingleOrDefault();
            if (result == null) return null;
            var repeatInfo = new RepeatInfo(result.Start, new DatePeriod(result.PeriodFrequency, result.PeriodQuantifier));
            return repeatInfo;
        }

        public static void InsertRepeatInfo(IDbConnection db, DatabaseDialect dialect, ScheduledJob job, RepeatInfo info)
        {
            const string sql = "INSERT INTO RepeatInfo " +
                               "(ScheduledJobId, PeriodFrequency, PeriodQuantifier, Start, IncludeWeekends) " +
                               "VALUES (@ScheduledJobId, @PeriodFrequency, @PeriodQuantifier, @Start, @IncludeWeekends);";

            db.Execute(sql, new
            {
                ScheduledJobId = job.Id,
                info.PeriodFrequency,
                info.PeriodQuantifier,
                info.Start,
                info.IncludeWeekends
            });
        }

        public static void UpdateRepeatInfo(IDbConnection db, DatabaseDialect dialect, ScheduledJob job, RepeatInfo info)
        {
            const string sql = "UPDATE RepeatInfo SET " +
                               "PeriodFrequency = @PeriodFrequency, " +
                               "PeriodQuantifier = @PeriodQuantifier, " +
                               "Start = @Start, " +
                               "IncludeWeekends = @IncludeWeekends " +
                               "WHERE ScheduledJobId = @ScheduledJobId;";

            db.Execute(sql, new
            {
                ScheduledJobId = job.Id,
                info.PeriodFrequency,
                info.PeriodQuantifier,
                info.Start,
                info.IncludeWeekends
            });
        }
    }
}