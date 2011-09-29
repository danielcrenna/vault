using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using metrics.Serialization;
using metrics.Util;

namespace metrics.Core
{
    /// <summary>
    /// A wrapper around available .NET performance counters
    /// <seealso href="http://msdn.microsoft.com/en-us/library/w8f5kw2e%28v=VS.71%29.aspx" />
    /// </summary>
    public static class CLRProfiler
    {
        private const string CategoryMemory = ".NET CLR Memory";
        private const string CategoryExceptions = ".NET CLR Exceptions";
        private const string CategoryLocksAndThreads = ".NET CLR LocksAndThreads";

        private static readonly Process _process;
        private static readonly IDictionary<Process, IDictionary<string, PerformanceCounter>> _counters;
        
        static CLRProfiler()
        {
            _process = Process.GetCurrentProcess();
            _counters = new Dictionary<Process, IDictionary<string, PerformanceCounter>>
                            {
                                { _process, new Dictionary<string, PerformanceCounter>() }
                            };
        }

        internal class ThreadInfo
        {
            public string Name { get; set; }
            public ThreadPriority Priority { get; set; }
            public IEnumerable<StackFrameInfo> StackFrames { get; set; }
        }

        internal class StackFrameInfo
        {
            public string Namespace { get; set; }
            public string MethodName { get; set; }
            public string FileName { get; set; }
            public int? LineNumber { get; set; }
            
            public StackFrameInfo(StackFrame frame)
            {
                SetStackMeta(frame);

                if (frame.GetILOffset() == -1)
                {
                    return;
                }

                string filename = null;
                try
                {
                    filename = frame.GetFileName();
                    if(filename != null)
                    {
                        LineNumber = frame.GetFileLineNumber();
                    }
                }
                catch (SecurityException)
                {

                }
               
                FileName = filename;
            }

            private void SetStackMeta(StackFrame frame)
            {
                var methodBase = frame.GetMethod();

                var sb = new StringBuilder();

                sb.Append(methodBase.DeclaringType.Namespace)
                    .Append(".")
                    .Append(methodBase.DeclaringType.Name)
                    .Append(".")
                    .Append(methodBase.Name)
                    .Append("(");

                var parameterInfo = methodBase.GetParameters();
                for (var i = 0; (i < parameterInfo.Length); i++)
                {
                    var parameterName = parameterInfo[i].ParameterType.Name;
                    sb.Append(String.Concat(((i != 0) ? ", " : ""), parameterName, " ", parameterInfo[i].Name));
                }

                sb.Append(")");

                MethodName = methodBase.DeclaringType.Name;
                Namespace = methodBase.DeclaringType.Namespace;
            }

            public static IEnumerable<StackFrameInfo> GetStackFrameInfo(IEnumerable<StackFrame> frames)
            {
                return frames.Select(f => new StackFrameInfo(f));
            }
        }

        /// <summary>
        /// Dumps all alive threads created via the <see cref="NamedThreadFactory" />
        /// </summary>
        /// <returns></returns>
        public static string DumpTrackedThreads()
        {
            var threads = NamedThreadFactory.Dump();

            var results = threads.Select(thread => new ThreadInfo
                                                       {
                                                           Name = thread.Name,
                                                           Priority = thread.Priority,
                                                           StackFrames = StackFrameInfo.GetStackFrameInfo(GetStackFramesForThread(thread))
                                                       });

            return Serializer.Serialize(results);
        }

        private static IEnumerable<StackFrame> GetStackFramesForThread(Thread thread)
        {
            StackTrace trace;
            switch (thread.ThreadState)
            {
                case System.Threading.ThreadState.Running:
                    thread.Suspend();
                    trace = new StackTrace(thread, true);
                    thread.Resume();
                    break;
                default:
                    trace = new StackTrace(thread, true);
                    break;
            }

            return trace.GetFrames();
        }

        #region Machine Metrics

        /*
        _Global_:.NET CLR LocksAndThreads:Total # of Contentions
        _Global_:.NET CLR LocksAndThreads:Contention Rate / sec
        _Global_:.NET CLR LocksAndThreads:Current Queue Length
        _Global_:.NET CLR LocksAndThreads:Queue Length Peak
        _Global_:.NET CLR LocksAndThreads:Queue Length / sec
        _Global_:.NET CLR LocksAndThreads:# of current logical Threads
        _Global_:.NET CLR LocksAndThreads:# of current physical Threads
        _Global_:.NET CLR LocksAndThreads:# of current recognized threads
        _Global_:.NET CLR LocksAndThreads:# of total recognized threads
        _Global_:.NET CLR LocksAndThreads:rate of recognized threads / sec
        */

        public static double MachineTotalContentions
        {
            get
            {
                var counter = GetOrInstallCounter("MachineTotalContentions", "Total # of Contentions", CategoryLocksAndThreads, "_Global_");
                var value = counter.NextValue();
                return value;
            }
        }

        public static double MachineContentionRatePerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("MachineContentionRatePerSecond", "Contention Rate / sec", CategoryLocksAndThreads, "_Global_");
                var value = counter.NextValue();
                return value;
            }
        }

        public static double MachineCurrentQueueLength
        {
            get
            {
                var counter = GetOrInstallCounter("MachineCurrentQueueLength", "Current Queue Length", CategoryLocksAndThreads, "_Global_");
                var value = counter.NextValue();
                return value;
            }
        }

        public static double MachineQueueLengthPeak
        {
            get
            {
                var counter = GetOrInstallCounter("MachineQueueLengthPeak", "Queue Length Peak", CategoryLocksAndThreads, "_Global_");
                var value = counter.NextValue();
                return value;
            }
        }                                                                                    


        #endregion


        private static PerformanceCounter GetOrInstallCounter(string property, string name, string category, string instance = null)
        {
            if (!_counters[_process].ContainsKey(property))
            {
                var counter = new PerformanceCounter(category, name, instance ?? _process.ProcessName, true);

                _counters[_process].Add(property, counter);
            }
            return _counters[_process][property];
        }
    }
}