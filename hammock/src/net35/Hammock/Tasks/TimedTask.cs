using System;
using System.Diagnostics;
using System.Threading;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif
namespace Hammock.Tasks
{
    public class TimedTask : ITimedTask
    {
        protected readonly object Lock = new object();
        protected bool Active;
        protected int Iterations;
        protected Timer Timer;
        protected bool ContinueOnError;

        public Action<bool> Action { get; protected set; }
        public Exception Exception { get; protected set; }
        public TimeSpan DueTime { get; protected set; }
        public TimeSpan Interval { get; protected set; }
        internal WebQueryAsyncResult AsyncResult { get; set; }
        public event Action<TimedTask, EventArgs> Stopped;

        public TimedTask(TimeSpan due,
                         TimeSpan interval,
                         int iterations,
                         bool continueOnError,
                         Action<bool> action) :
            this(due, interval, iterations, action)
        {
            ContinueOnError = continueOnError;
        }

        public TimedTask(TimeSpan due,
                         TimeSpan interval,
                         int iterations,
                         Action<bool> action)
        {
            DueTime = due;
            Interval = interval;
            Iterations = iterations;
            Action = action;
        }

        protected virtual void Start(bool continueOnError)
        {
            var count = 0;
            Timer = new Timer(state =>
            {
                try
                {
                    Action(false);
                    count++;
                    if (Iterations > 0 && count > Iterations)
                    {
                        Stop();
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    if (!continueOnError)
                    {
                        Stop();
                    }
                }
            }, null, DueTime, Interval);
        }


        public virtual void Stop()
        {
            if (Active)
            {
                lock (Lock)
                {
                    if (Active)
                    {
                        Active = false;
                        Timer.Change(-1, -1);
                        OnStopped(EventArgs.Empty);
                        if (AsyncResult != null)
                        {
                            AsyncResult.Signal();
                        }
                    }
                }
            }
        }

        public virtual void Start()
        {
            if (!Active)
            {
                lock (Lock)
                {
                    if (!Active)
                    {
                        Active = true;
                        if (Timer != null)
                        {
                            Timer.Change(DueTime, Interval);
                        }
                        else
                        {
                            Start(ContinueOnError);
                        }
                    }
                }
            }
        }

        public virtual void Start(TimeSpan dueTime, TimeSpan interval)
        {
            if (!Active)
            {
                lock (Lock)
                {
                    if (!Active)
                    {
                        DueTime = dueTime;
                        Interval = interval;
                        Timer.Change(DueTime, Interval);
                    }
                }
            }
        }
        protected virtual void OnStopped(EventArgs e)
        {
            if (Stopped != null)
            {
                Stopped(this, e);
            }
        }

        public virtual void Dispose()
        {
            Stop();
            Timer.Dispose();
        }
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public class TimedTask<T> : TimedTask, ITimedTask<T>
    {
        public TimedTask(TimeSpan due,
                         TimeSpan interval,
                         int iterations,
                         bool continueOnError,
                         Action<bool> action,
                         IRateLimitingRule<T> rateLimitingRule) :
            base(due, interval, iterations, action)
        {
            RateLimitingRule = rateLimitingRule;
            ContinueOnError = continueOnError;
        }

        protected override void Start(bool continueOnError)
        {
            var count = 0;
            Timer = new Timer(state =>
            {
                try
                {
                    //[JD] 
                    //only allow the task to run once concurrently. 
                    //if a second task attempts to enter the monitor while
                    //the first one is still running, simply drop it 
                    if (Monitor.TryEnter(Lock))
                    {
                        try
                        {
#if TRACE
                            Trace.WriteLine("Running a periodic task");
#endif
                            var skip = RateLimitingRule.ShouldSkipForRateLimiting();
#if TRACE
                            Trace.WriteLine(string.Format("{0} Evaluated rate limiting predicate and result was {1}",
                                                          DateTime.Now.ToShortTimeString(),
                                                          skip ? "'skip'" : "'don't skip'"));
#endif
                            Action(skip);
                            var newInterval = RateLimitingRule.CalculateNewInterval();
#if TRACE
                            Trace.WriteLine(string.Format("{0} Calculated new interval for throttled task and result was: {1}",
                                                          DateTime.Now.ToShortTimeString(),
                                                          newInterval.HasValue ? newInterval.Value.ToString() : "'no change'"));
#endif
                            count++;

                            if (Iterations > 0 && count >= Iterations)
                            {
                                Stop();
                            }
                            else if (newInterval.HasValue)
                            {
                                Timer.Change((int)newInterval.Value.TotalMilliseconds, (int)newInterval.Value.TotalMilliseconds);
                            }
                        }
                        finally
                        {
                            Monitor.Exit(Lock);
                        }
                    }
                    else
                    {
#if TRACE
                        Trace.WriteLine("Skipping recurring task because the previous iteration is still active");
#endif
                        Action(true);
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    if (!continueOnError)
                    {
                        Stop();
                    }
                }
            }, null, DueTime, Interval);
        }

        #region ITimedTask Members

        public virtual bool RateLimited
        {
            get { return RateLimitingRule != null; }
        }

        public IRateLimitingRule<T> RateLimitingRule { get; set; }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }
}