using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Web.Hosting;
using ImpromptuInterface;
using Timer = System.Threading.Timer;

namespace linger
{
    /// <summary>
    /// A task worker that watches a job queue and performs work. Jobs are scheduled by priority.
    /// </summary>
    public class LingerWorker : IRegisteredObject
    {
        private static readonly IDictionary<Type, IList<string>> Cache = new ConcurrentDictionary<Type, IList<string>>(); 

        private readonly ConcurrentDictionary<int, TaskScheduler> _schedulers;
        private readonly ConcurrentDictionary<TaskScheduler, TaskFactory> _factories;
        private readonly ConcurrentDictionary<Perform, IList<string>> _pending;
        
        private QueuedTaskScheduler _queue;
        private CancellationTokenSource _cancel;
        private readonly int _threads;
        private static IDisposable _background;

        public LingerWorker(int threads)
        {
            _threads = threads;
            _schedulers = new ConcurrentDictionary<int, TaskScheduler>();
            _factories = new ConcurrentDictionary<TaskScheduler, TaskFactory>();
            _pending = new ConcurrentDictionary<Perform, IList<string>>();
            _cancel = new CancellationTokenSource();
        }

        public void Start()
        {
            if(_queue == null)
            {
                _queue = new QueuedTaskScheduler(_threads);
            }

            // This probaby uses a worker pool thread, so should be replaced eventually
            _background = new Timer(state => SeedJobsFromQueue(), null, 0, Linger.SleepDelay * 1000);
        }

        public void Stop(bool immediate)
        {
            foreach (var entry in _pending.Where(entry => entry.Value.Contains("Halt")))
            {
                entry.Key.ActLike<Halt>().Halt(immediate);
            }
            if (immediate)
            {
                Stop();
            }
        }

        public void Stop()
        {
            if(_queue != null)
            {
                _queue.Dispose();
                _queue = null;
            }
            if(_background != null)
            {
                _background.Dispose();
                _background = null;
            }
        }
        
        private void SeedJobsFromQueue()
        {
            var jobs = Linger.Backend.GetNextAvailable(Linger.ReadAhead);

            var pending = new Dictionary<Task, CancellationTokenSource>();
            foreach(var job in jobs)
            {
                var scheduler = AcquireScheduler(job);
                var scheduled = job;

                var cancel = new CancellationTokenSource();
                var performer = _factories[scheduler].StartNew(() =>
                {
                    AttemptJob(scheduled);
                }, cancel.Token);
                
                pending.Add(performer, cancel);
            }

            // This is effectively giving all tasks below the current as much time as the current...
            foreach(var performer in pending)
            {
                if (!Task.WaitAll(new[] { performer.Key }, Linger.MaximumRuntime))
                {
                    performer.Value.Cancel();
                }
            }
        }

        internal bool AttemptJob(ScheduledJob job, bool persist = true)
        {
            if(_cancel.IsCancellationRequested)
            {
                return false;
            }

            var success = AttemptCycle(job);

            if(persist)
            {
                SaveJobChanges(job, success);
            }
            
            _cancel.Token.ThrowIfCancellationRequested();

            return success;
        }

        private bool AttemptCycle(ScheduledJob job)
        {
            job.Attempts++;
            var success = Perform(job);
            if (!success)
            {
                var dueTime = DateTime.Now + Linger.IntervalFunction(job.Attempts);
                job.RunAt = dueTime;
            }
            return success;
        }

        private static void SaveJobChanges(ScheduledJob job, bool success)
        {
            if (!success)
            {
                if(JobWillFail(job))
                {
                    if (Linger.DeleteFailedJobs)
                    {
                        Linger.Backend.Delete(job);
                        return;
                    }
                    job.FailedAt = DateTime.Now;
                }
            }
            else
            {
                if(Linger.DeleteSuccessfulJobs)
                {
                    Linger.Backend.Delete(job);
                    return;
                }
                job.SucceededAt = DateTime.Now;
            }

            job.LockedAt = null;
            job.LockedBy = null;
            Linger.Backend.Save(job);

            // Spawn a new scheduled job using the repeat data
            if (success && job.RepeatInfo != null && job.RepeatInfo.NextOccurrence.HasValue)
            {
                job.Id = 0;
                job.RunAt = job.RepeatInfo.NextOccurrence;
                job.RepeatInfo.Start = job.RunAt;
                Linger.Backend.Save(job, job.RepeatInfo);
            }
        }

        private static bool JobWillFail(ScheduledJob job)
        {
            return job.Attempts >= Linger.MaximumAttempts;
        }

        private bool Perform(ScheduledJob job)
        {
            var success = false;
            Perform handler = null;
            IList<string> methods = null;
            
            try
            {
                // Acquire the handler
                handler = HandlerSerializer.Deserialize<object>(job.Handler).ActLike<Perform>();
                if (handler == null)
                {
                    job.LastError = "Missing handler";
                    return false;
                }

                // Acquire and cache method manifest
                var handlerType = handler.GetType();
                if (!Cache.TryGetValue(handlerType, out methods))
                {
                    methods = handlerType.GetMethods().Select(m => m.Name).ToList();
                    Cache.Add(handlerType, methods);
                }
                
                _pending.TryAdd(handler, methods);
                
                // Before
                if(methods.Contains("Before"))
                {
                    handler.ActLike<Before>().Before();
                }

                // Perform
                success = handler.Perform();

                if(success)
                {
                    if (methods.Contains("Success"))
                    {
                        handler.ActLike<Success>().Success();
                    }
                }

                // Failure
                if(JobWillFail(job) && methods.Contains("Failure"))
                {
                    {
                        handler.ActLike<Failure>().Failure();
                    }
                }

                // After
                if(methods.Contains("After"))
                {
                    handler.ActLike<After>().After();
                }

                _pending.TryRemove(handler, out methods);
            }
            catch (OperationCanceledException)
            {
                job.LastError = "Cancelled";
            }
            catch (Exception ex)
            {
                job.LastError = ex.Message;
                if (methods != null && methods.Contains("Error"))
                {
                    handler.ActLike<Error>().Error(ex);
                }
            }
            return success;
        }

        private TaskScheduler AcquireScheduler(ScheduledJob job)
        {
            TaskScheduler scheduler;
            if (!_schedulers.TryGetValue(job.Priority, out scheduler))
            {
                scheduler = _queue.ActivateNewQueue(job.Priority);
                var factory = new TaskFactory(
                    _cancel.Token, TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning, scheduler
                    );
                _schedulers.TryAdd(job.Priority, scheduler);
                _factories.TryAdd(scheduler, factory);
            }
            return scheduler;
        }

        public void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if(_cancel != null)
            {
                _cancel.Cancel();
                _cancel.Token.WaitHandle.WaitOne();
                _cancel.Dispose();
                _cancel = null;
            }
            _factories.Clear();
            _schedulers.Clear();
            if (_queue == null)
            {
                return;
            }
            _queue.Dispose();
            _queue = null;
        }
    }
}

