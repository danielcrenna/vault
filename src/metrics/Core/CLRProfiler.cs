using System.Collections.Generic;
using System.Diagnostics;

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

        public static string DumpThreads()
        {
            return "Not implemented; how about a fork?";
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