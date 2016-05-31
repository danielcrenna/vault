using System;

namespace linger
{
    public static class LingerExtensions
    {
        public static void PerformAsync(this Perform perform, int? priority = null)
        {
            Linger.QueueForExecution(perform, priority ?? Linger.DefaultPriority);
        }

        public static void PerformAsync(this Perform perform, DateTime runAt, int? priority = null)
        {
            Linger.QueueForExecution(perform, priority ?? Linger.DefaultPriority, runAt);
        }

        public static void PerformAsync(this Func<bool> job, int? priority = null)
        {
            Linger.QueueForExecution(new DelegateJob(job), priority ?? Linger.DefaultPriority);
        }

        public static void PerformAsync(this Func<bool> job, DateTime runAt, int? priority = null)
        {
            Linger.QueueForExecution(new DelegateJob(job), priority ?? Linger.DefaultPriority, runAt);
        }

        public static void PerformAsync(this Action job, int? priority = null)
        {
            var inner = new DelegateJob(() => { job();  return true; });
            Linger.QueueForExecution(inner, priority ?? Linger.DefaultPriority);
        }

        public static void PerformAsync(this Action job, DateTime runAt, int? priority = null)
        {
            var inner = new DelegateJob(() => { job(); return true; });
            Linger.QueueForExecution(inner, priority ?? Linger.DefaultPriority, runAt);
        }
    }
}