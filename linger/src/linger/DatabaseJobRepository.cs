using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace linger
{
    /// <summary>
    /// Stores the job queue in an ADO-NET supported database. Currently supports SQL Server, MySQL, and SQLite dialects.
    /// </summary>
    public class DatabaseJobRepository : IScheduledJobRepository
    {
        private static readonly object Sync = new object();
        private readonly DatabaseDialect _dialect;
        private readonly Func<IDbConnection> _connectionBuilder;

        public DatabaseJobRepository(DatabaseDialect dialect, Func<IDbConnection> connectionBuilder)
        {
            _dialect = dialect;
            _connectionBuilder = connectionBuilder;
        }

        public void InstallSchema()
        {
            Schema.Install(_dialect, _connectionBuilder());
        }

        public void Save(ScheduledJob job, RepeatInfo info = null)
        {
            var db = _connectionBuilder();
            if(job.Id == 0)
            {
                Queries.InsertJob(db, _dialect, job);
            }
            else
            {
                job.UpdatedAt = DateTime.Now;
                Queries.UpdateJob(db, job);
            }

            var existing = Queries.GetRepeatInfo(db, job);
            if(existing == null)
            {
                Queries.InsertRepeatInfo(db, _dialect, job, info);
            }
            else
            {
                Queries.UpdateRepeatInfo(db, _dialect, job, info);
            }
        }
        
        public ScheduledJob Load(int id)
        {
            var db = _connectionBuilder();
            var job = Queries.GetJob(db, id);
            if(job != null)
            {
                job.RepeatInfo = Queries.GetRepeatInfo(db, job);
            }
            return job;
        }
        
        public void Delete(ScheduledJob job)
        {
            var db = _connectionBuilder();
            Queries.DeleteJob(db, job);
        }

        public IList<ScheduledJob> GetNextAvailable(int readAhead)
        {
            var db = _connectionBuilder();
            lock (Sync)
            {
                var jobs = Queries.GetNextAvailable(db, _dialect, readAhead).ToList();
                if(jobs.Any())
                {
                    Queries.LockJobs(db, jobs);
                }
                return jobs;                
            }
        }

        public IList<ScheduledJob> GetAll()
        {
            var db = _connectionBuilder();
            lock (Sync)
            {
                var jobs = Queries.GetAll(db).ToList();
                return jobs;                
            }
        }

        public Batch CreateBatch(string name, IEnumerable<ScheduledJob> jobs)
        {
            var db = _connectionBuilder();
            var batch = new Batch { Name = name, Jobs = jobs };
            Queries.InsertBatch(db, _dialect, batch);
            foreach(var job in batch.Jobs)
            {
                if (job.Priority != batch.Priority)
                {
                    job.Priority = batch.Priority;
                    Save(job);
                }
                Queries.AddToBatch(db, _dialect, batch, job);
            }
            return batch;
        }
    }
}