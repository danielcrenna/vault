using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;
using NLog.Common;
using NLog.Targets;
using StackExchange.Profiling;
using bulky;
using tuxedo.Dapper;

namespace cohort.Logging
{
    // http://nlog-project.org/wiki/How_to_write_a_Target

    /// <summary>
    /// A trap for all log statements. Logs are captured and flushed
    /// at the per user level if a request context is available,
    /// otherwise they are are written immediately.
    /// </summary>
    [Target("CohortLog")]
    public class CohortLogTarget : TargetWithLayout
    {
        private const string CohortLogKey = "__Cohort__Logs";
        
        public static void Flush()
        {
            var logs = GetOrderedLogsInQueue().ToList();
            if (logs.Count == 0) return;

            // We don't want multiple logging inserts to look like bad performance
            using (MiniProfiler.Current.Ignore())
            {
                Cohort.Database.BulkCopy(logs);
            }    
        }

        private static IEnumerable<Log> GetOrderedLogsInQueue()
        {
            lock (HttpContext.Current.Items.SyncRoot)
            {
                var events = new List<LogEventInfo>();
                var queue = HttpContext.Current.Items[CohortLogKey] as Queue<LogEventInfo>;
                while (queue != null && queue.Count > 0)
                {
                    events.Add(queue.Dequeue());
                }
                return events.OrderBy(e => e.SequenceID).Select(ProjectLog);
            }
        }
        
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            try
            {
                // The check for the request is a dirty way to check if we're trying to log from App_Start
                if(HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    EnqueueLogEvent(logEvent);
                    return;
                }
            }
            catch (HttpException)
            {
                // We're in App_Start, so we'll just write the log directly    
            }
            using (MiniProfiler.Current.Ignore())
            {
                Cohort.Database.Insert(ProjectLog(logEvent.LogEvent));
            }
            base.Write(logEvent);
        }

        private static void EnqueueLogEvent(AsyncLogEventInfo logEvent)
        {
            lock (HttpContext.Current.Items.SyncRoot)
            {
                if (HttpContext.Current.Items[CohortLogKey] == null)
                {
                    HttpContext.Current.Items[CohortLogKey] = new Queue<LogEventInfo>();
                }
                var queue = (Queue<LogEventInfo>) HttpContext.Current.Items[CohortLogKey];
                queue.Enqueue(logEvent.LogEvent);
            }
        }

        private static Log ProjectLog(LogEventInfo logEvent)
        {
            var log = new Log
            {
                Level = logEvent.Level.Name,
                Message = logEvent.FormattedMessage,
                StackTrace = logEvent.HasStackTrace ? logEvent.StackTrace.ToString() : null,
                User = Cohort.User != null ? Cohort.User.Identity : null,
                IPAddress = Cohort.User != null ? Cohort.User.IPAddress : null
            };

            if (HttpContext.Current != null)
            {
                try
                {
                    log.Path = HttpContext.Current.Request.Url.PathAndQuery;
                }
                catch (HttpException)
                {
                    // Squash errors when log is called in Application_Start   
                }
            }
            return log;
        }
    }
}