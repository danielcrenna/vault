using System;
using System.Threading;
using metrics.Support;

namespace metrics.Util
{
    /// <summary>
    /// A simple named thread factory
    /// </summary>
    public class NamedThreadFactory
    {
        private long _number = 1;
        private readonly string _prefix;
        private static readonly ThreadGroup _group;

        static NamedThreadFactory()
        {
            _group = new ThreadGroup("NamedThreadFactory");
        }

        public NamedThreadFactory(string name)
        {
            _prefix = string.Concat(name, "-thread-");
        }

        public Thread New(Action closure)
        {
            // .NET doesn't manage threads in groups, so likely need to find a better way 
            // to watch spawned child threads if the system won't do it for us

            /* final SecurityManager s = System.getSecurityManager();
		       this.group = (s != null) ? s.getThreadGroup() : Thread.currentThread().getThreadGroup(); */

            var thread = new Thread(new ThreadStart(closure));
            var number = Interlocked.Read(ref _number);
            Interlocked.Increment(ref _number);
            thread.Name = string.Concat(_prefix, number);
            thread.Priority = ThreadPriority.Normal;

            _group.Add(thread);
            return thread;
        }
    }
}

