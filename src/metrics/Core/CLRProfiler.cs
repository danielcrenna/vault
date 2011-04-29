using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace metrics.Core
{
    public static class CLRProfiler
    {
        private const string CategoryMemory = ".NET CLR Memory";
        private static readonly Process _process;
        private static readonly IDictionary<Process, IDictionary<string, PerformanceCounter>> _counters;
        
        static CLRProfiler()
        {
            _process = Process.GetCurrentProcess();
            _counters = new Dictionary<Process, IDictionary<string, PerformanceCounter>>
                            {
                                {_process, new Dictionary<string, PerformanceCounter>()}
                            };
        }

        public static string DumpThreads()
        {
            return "Not implemented; how about a fork?";
        }
        
        /// <summary>
        /// Returns the number of seconds the CLR process has been running
        /// </summary>
        public static long Uptime
        {
            get { return Convert.ToInt64(_process.TotalProcessorTime.TotalSeconds); }
        }
        
        /// <summary>
        /// Returns the percentage of the CLR's heap which is being used
        /// </summary>
        public static double HeapUsage
        {
            get
            {
                var counter = GetOrInstallCounter("HeapUsage", CategoryMemory);
                var used = WaitForNextRawValue(counter);

                var available = _process.PrivateMemorySize64;
                var usage = (double)used / available;
                return usage;
            }
        }

        private static long WaitForNextRawValue(PerformanceCounter counter)
        {
            long used;
            while((used = counter.NextSample().RawValue) == 0)
            {
                Thread.Sleep(10);
            }
            return used;
        }

        private static PerformanceCounter GetOrInstallCounter(string property, string category)
        {
            if (!_counters[_process].ContainsKey(property))
            {
                _counters[_process].Add(property, new PerformanceCounter(category, "# bytes in all heaps", _process.ProcessName));
            }
            return _counters[_process][property];
        }
    }
}