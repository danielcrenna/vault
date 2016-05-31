using System;
using System.Collections.Generic;
using System.Threading;
using metrics.Support;

namespace metrics.Util
{
    /// <summary>
    /// A simple named thread factory, used to track interesting threads (whose traces can be dumped).
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
            _prefix = string.Concat(name, "_thread-");
        }

        /// <summary>
        /// Dumps all threads that have been added via <see cref="New" />.
        /// <remarks>
        ///     - It's not possible to enumerate managed CLR threads at runtime in code
        /// </remarks>
        /// </summary> 
        /// <returns></returns>
        public static IEnumerable<Thread> Dump()
        {
            return _group;
        }

        public Thread New(Action closure, ThreadPriority priority = ThreadPriority.Normal)
        {
            var thread = new Thread(new ThreadStart(closure));
            var number = Interlocked.Read(ref _number);
            Interlocked.Increment(ref _number);
            thread.Name = string.Concat(_prefix, number);
            thread.Priority = priority;

            _group.Add(thread);
            return thread;
        }
    }
}

