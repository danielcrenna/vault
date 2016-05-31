using System;

namespace linger
{
    partial class Linger
    {
        /// <summary>
        /// The maximum time each task is allowed before being cancelled; default is 4 hours
        /// </summary>
        public static TimeSpan MaximumRuntime { get; set; }

        /// <summary>
        /// The maximum number of attempts made before failing a job permanently; default is 25
        /// </summary>
        public static int MaximumAttempts { get; set; }

        /// <summary>
        /// Whether or not failed jobs are deleted from the backend store; default is false
        /// </summary>
        public static bool DeleteFailedJobs { get; set; }

        /// <summary>
        /// Whether or not successful jobs are deleted from the backend store; default is false
        /// </summary>
        public static bool DeleteSuccessfulJobs { get; set; }

        /// <summary>
        /// The function responsible for calculating the next attempt date after a job fails;
        /// default is 5 seconds + N.Pow(4), where N is the number of retries
        /// </summary>
        public static Func<int, TimeSpan> IntervalFunction { get; set; }

        /// <summary>
        /// The number of jobs to pull at once when searching for available jobs; default is 5
        /// </summary>
        public static int ReadAhead { get; set; }

        /// <summary>
        /// The number of seconds to delay before checking for available jobs in the backing store
        /// </summary>
        public static int SleepDelay { get; set; }

        /// <summary>
        /// Whether or not jobs are delayed or executed immediately; default is true
        /// </summary>
        public static bool DelayJobs { get; set; }

        /// <summary>
        /// The default priority level for newly created scheduled jobs that don't specify a priority;
        /// default is 0, or highest priority
        /// </summary>
        public static int DefaultPriority { get; set; }

        /// <summary>
        /// The number of threads available for performing jobs; default is 5
        /// </summary>
        public static int Concurrency { get; set; }

        /// <summary>
        /// The backend used to coordinate jobs; there is no default
        /// <example>
        /// Linger.Backend = new DatabaseJobRepository(DatabaseDialect.Sqlite, () => UnitOfWork.Current);
        /// </example>
        /// </summary>
        public static IScheduledJobRepository Backend { get; set; }

        static Linger()
        {
            MaximumAttempts = 25;
            MaximumRuntime = TimeSpan.FromHours(4);
            DeleteFailedJobs = false;
            DeleteSuccessfulJobs = false;
            IntervalFunction = i => TimeSpan.FromSeconds(5 + (Math.Pow(i, 4)));
            ReadAhead = 5;
            SleepDelay = 60;
            DelayJobs = true;
            DefaultPriority = 0;
            Concurrency = 5;
        }
    }
}
