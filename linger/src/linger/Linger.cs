using System;
using System.Web.Hosting;

namespace linger
{
    public partial class Linger
    {
        private static LingerWorker _worker;
        
        public static void StartWorker()
        {
            EnsureBackend();
            StopWorker();
            CreateAndStartWorker();
        }

        private static void CreateAndStartWorker()
        {
            if(_worker != null)
            {
                throw new LingerException("Worker already exists, and was not properly stopped");
            }
            _worker = new LingerWorker(Concurrency);
            if (HostingEnvironment.IsHosted)
            {
                HostingEnvironment.RegisterObject(_worker);
            }
            _worker.Start();
        }

        public static void StopWorker()
        {
            if (_worker == null) return;
            if (HostingEnvironment.IsHosted)
            {
                HostingEnvironment.UnregisterObject(_worker);
            }
            _worker.Stop();
            _worker.Dispose(true);
            _worker = null;
        }
        
        public static bool QueueForExecution(dynamic job, int? priority = null, DateTime? runAt = null, Func<ScheduledJob, RepeatInfo> repeat = null)
        {
            var scheduledJob = CreateScheduledJob(job, priority ?? DefaultPriority, runAt);
            var repeatInfo = repeat != null ? repeat(scheduledJob) : null;

            if(DelayJobs)
            {
                EnsureBackend();
                Backend.Save(scheduledJob, repeatInfo);
                return true;
            }
            if (_worker == null)
            {
                CreateAndStartWorker();
            }
            return _worker != null && _worker.AttemptJob(scheduledJob, persist: false);
        }

        private static void EnsureBackend()
        {
            if (Backend == null)
            {
                throw new LingerException("No backend function found.");
            }
        }

        internal static ScheduledJob CreateScheduledJob(dynamic job, int priority, DateTime? runAt)
        {
            var scheduled = new ScheduledJob
            {
                Priority = priority,
                Handler = HandlerSerializer.Serialize(job),
                RunAt = runAt
            };
            return scheduled;
        }
    }
}