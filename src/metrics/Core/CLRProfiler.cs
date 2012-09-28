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
        private const string CategoryNetworking = ".NET CLR Networking";
        private const string GlobalInstance = "_Global_";

        private static readonly string Process;
        private static readonly IDictionary<string, IDictionary<string, PerformanceCounter>> Counters;
        
        static CLRProfiler()
        {
            Process = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Counters = new Dictionary<string, IDictionary<string, PerformanceCounter>>
                            {
                                { Process, new Dictionary<string, PerformanceCounter>() }
                            };
        }

        /// <summary>
        /// Clears all lazily-initialized performance counters from memory
        /// </summary>
        public static void ClearCounters()
        {
            Counters.Clear();
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

        internal class ThreadInfo
        {
            public string Name { internal get; set; }
            public ThreadPriority Priority { internal get; set; }
            public IEnumerable<StackFrameInfo> StackFrames { internal get; set; }
        }

        internal class StackFrameInfo
        {
            public string Namespace { get; set; }
            public string MethodName { get; set; }
            public string FileName { get; set; }
            public int? LineNumber { get; set; }

            private StackFrameInfo(StackFrame frame)
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
                    if (filename != null)
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

        #region Global Metrics

        #region .NET CLR Memory ("http://msdn.microsoft.com/en-us/library/x2tyfybc.aspx")
        
        /// <summary>
        /// Displays the sum of the Gen 1 Heap Size, Gen 2 Heap Size, and the Large Object Heap Size counters. This counter indicates the current memory allocated in bytes on the garbage collection heaps.
        /// </summary>
        public static double GlobalNumberOfBytesInAllHeaps
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfBytesInAllHeaps", "# Bytes in all Heaps", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current number of garbage collection handles in use. Garbage collection handles are handles to resources external to the common language runtime and the managed environment.
        /// </summary>
        public static double GlobalNumberOfGCHandles
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfGCHandles", "# GC Handles", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of times the generation 0 objects (that is, the youngest, most recently allocated objects) are garbage collected since the application started.
        /// Generation 0 garbage collection occurs when the available memory in generation 0 is not sufficient to satisfy an allocation request. This counter is incremented at the end of a generation 0 garbage collection. Higher generation garbage collections include all lower generation collections. This counter is explicitly incremented when a higher generation (generation 1 or 2) garbage collection occurs.
        /// This counter displays the last observed value. The _Global_ counter value is not accurate and should be ignored.
        /// </summary>
        public static double GlobalNumberOfGen0Collections
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfGen0Collections", "# Gen 0 Collections", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of times the generation 1 objects are garbage collected since the application started.
        /// The counter is incremented at the end of a generation 1 garbage collection. Higher generation garbage collections include all lower generation collections. This counter is explicitly incremented when a higher generation (generation 2) garbage collection occurs.
        /// This counter displays the last observed value. The _Global_ counter value is not accurate and should be ignored.
        /// </summary>
        public static double GlobalNumberOfGen1Collections
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfGen1Collections", "# Gen 1 Collections", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of times the generation 2 objects are garbage collected since the application started. The counter is incremented at the end of a generation 2 garbage collection (also called a full garbage collection).
        /// This counter displays the last observed value. The _Global_ counter value is not accurate and should be ignored.
        /// </summary>
        public static double GlobalNumberOfGen2Collections
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfGen2Collections", "# Gen 2 Collections", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the peak number of times garbage collection was performed because of an explicit call to GC.Collect. It is good practice to let the garbage collector tune the frequency of its collections.
        /// </summary>
        public static double GlobalNumberOfInducedGC
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfInducedGC", "# Induced GC", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of pinned objects encountered in the last garbage collection. A pinned object is one that the garbage collector cannot move in memory. This counter tracks the pinned objects only in the heaps that are garbage collected. For example, a generation 0 garbage collection causes enumeration of pinned objects only in the generation 0 heap.
        /// </summary>
        public static double GlobalNumberOfPinnedObjects
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfPinnedObjects", "# of Pinned Objects", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current number of syncronization blocks in use. Synchronization blocks are per-object data structures allocated for storing synchronization information. Synchronization blocks hold weak references to managed objects and must be scanned by the garbage collector. Synchronization blocks are not limited to storing synchronization information; they can also store COM interop metadata. This counter indicates performance problems with heavy use of synchronization primitives.
        /// </summary>
        public static double GlobalNumberOfSinkBlocksInUse
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfSinkBlocksInUse", "# of Sink Blocks in use", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the amount of virtual memory, in bytes, currently committed by the garbage collector. Committed memory is the physical memory for which space has been reserved in the disk paging file.
        /// </summary>
        public static double GlobalNumberOfTotalCommittedBytes
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfTotalCommittedBytes", "# Total committed Bytes", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the amount of virtual memory. in bytes, currently reserved by the garbage collector. Reserved memory is the virtual memory space reserved for the application but no disk or main memory pages have been used.
        /// </summary>
        public static double GlobalNumberOfTotalReservedBytes
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfTotalReservedBytes", "# Total reserved Bytes", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the percentage of elapsed time that was spent performing a garbage collection since the last garbage collection cycle. This counter usually indicates the work done by the garbage collector to collect and compact memory on behalf of the application. This counter is updated only at the end of every garbage collection. This counter is not an average; its value reflects the last observed value.
        /// </summary>
        public static double GlobalPercentTimeInGC
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalPercentTimeInGC", "% Time in GC", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of bytes per second allocated on the garbage collection heap. This counter is updated at the end of every garbage collection, not at each allocation. This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double GlobalAllocatedBytesPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalAllocatedBytesPerSecond", "Allocated Bytes/second", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of garbage-collected objects that survive a collection because they are waiting to be finalized. If these objects hold references to other objects, those objects also survive but are not counted by this counter. The Promoted Finalization-Memory from Gen 0 and Promoted Finalization-Memory from Gen 1 counters represent all the memory that survived due to finalization.    
        /// This counter is not cumulative; it is updated at the end of every garbage collection with the count of the survivors during that particular collection only. This counter indicates the extra overhead that the application might incur because of finalization.
        /// </summary>
        public static double GlobalFinalizationSurvivors
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalFinalizationSurvivors", "Finalization Survivors", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the maximum bytes that can be allocated in generation 0; it does not indicate the current number of bytes allocated in generation 0.
        /// A generation 0 garbage collection occurs when the allocations since the last collection exceed this size. The generation 0 size is tuned by the garbage collector and can change during the execution of the application. At the end of a generation 0 collection the size of the generation 0 heap is 0 bytes. This counter displays the size, in bytes, of allocations that invokes the next generation 0 garbage collection.
        /// This counter is updated at the end of a garbage collection, not at each allocation.
        /// </summary>
        public static double GlobalGen0HeapSize
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalGen0HeapSize", "Gen 0 heap size", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes per second that are promoted from generation 0 to generation 1. Memory is promoted when it survives a garbage collection. This counter is an indicator of relatively long-lived objects being created per second.
        /// This counter displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double GlobalGen0PromotedBytesPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalGen0PromotedBytesPerSecond", "Gen 0 Promoted Bytes/Sec", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current number of bytes in generation 1; this counter does not display the maximum size of generation 1. Objects are not directly allocated in this generation; they are promoted from previous generation 0 garbage collections. This counter is updated at the end of a garbage collection, not at each allocation.
        /// </summary>
        public static double GlobalGen1HeapSize
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalGen1HeapSize", "Gen 1 heap size", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes per second that are promoted from generation 1 to generation 2. Objects that are promoted only because they are waiting to be finalized are not included in this counter.
        /// Memory is promoted when it survives a garbage collection. Nothing is promoted from generation 2 because it is the oldest generation. This counter is an indicator of very long-lived objects being created per second.
        /// This counter displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double GlobalGen1PromotedBytesPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalGen1PromotedBytesPerSecond", "Gen 1 Promoted Bytes/Sec", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current number of bytes in generation 2. Objects are not directly allocated in this generation; they are promoted from generation 1 during previous generation 1 garbage collections. This counter is updated at the end of a garbage collection, not at each allocation.
        /// </summary>
        public static double GlobalGen2HeapSize
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalGen2HeapSize", "Gen 2 heap size", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current size, in bytes, of the Large Object Heap. Objects that are greater than approximately 85,000 bytes are treated as large objects by the garbage collector and are directly allocated in a special heap; they are not promoted through the generations. This counter is updated at the end of a garbage collection, not at each allocation.
        /// </summary>
        public static double GlobalLargeObjectHeapSize
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalLargeObjectHeapSize", "Large Object Heap size", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes of memory that are promoted from generation 0 to generation 1 only because they are waiting to be finalized. This counter is not cumulative; it displays the value observed at the end of the last garbage collection.
        /// </summary>
        public static double GlobalPromotedFinalizationMemoryFromGen0
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalPromotedFinalizationMemoryFromGen0", "Promoted Finalization-Memory from Gen 0", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes of memory that survive garbage collection and are promoted from generation 0 to generation 1. Objects that are promoted only because they are waiting to be finalized are not included in this counter. This counter is not cumulative; it displays the value observed at the end of the last garbage collection.
        /// </summary>
        public static double GlobalPromotedMemoryFromGen0
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalPromotedMemoryFromGen0", "Promoted Memory from Gen 0", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes of memory that survive garbage collection and are promoted from generation 1 to generation 2. Objects that are promoted only because they are waiting to be finalized are not included in this counter. This counter is not cumulative; it displays the value observed at the end of the last garbage collection. This counter is reset to 0 if the last garbage collection was a generation 0 collection only.
        /// </summary>
        public static double GlobalPromotedMemoryFromGen1
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalPromotedMemoryFromGen1", "Promoted Memory from Gen 1", CategoryMemory, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }
        
        #endregion

        #region .NET CLR Locks And Threads ("http://msdn.microsoft.com/en-us/library/zf749bat(v=VS.71).aspx")
        
        /// <summary>
        /// Displays the number of current managed thread objects in the application. This counter maintains the count of both running and stopped threads. This counter is not an average over time; it displays only the last observed value.
        /// </summary>
        public static double GlobalNumberOfCurrentLogicalThreads
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfCurrentLogicalThreads", "# of current logical Threads", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of native operating system threads created and owned by the common language runtime to act as underlying threads for managed thread objects. This counter's value does not include the threads used by the runtime in its internal operations; it is a subset of the threads in the operating system process.
        /// </summary>
        public static double GlobalNumberOfCurrentPhysicalThreads
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfCurrentPhysicalThreads", "# of current physical Threads", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of threads that are currently recognized by the runtime. These threads are associated with a corresponding managed thread object. The runtime does not create these threads, but they have run inside the runtime at least once.
        /// Only unique threads are tracked; threads with the same thread ID that reenter the runtime or are recreated after the thread exits are not counted twice.
        /// </summary>
        public static double GlobalNumberOfCurrentRecognizedThreads
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfCurrentRecognizedThreads", "# of current recognized threads", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the total number of threads that have been recognized by the runtime since the application started. These threads are associated with a corresponding managed thread object. The runtime does not create these threads, but they have run inside the runtime at least once.
        /// Only unique threads are tracked; threads with the same thread ID that reenter the runtime or are recreated after the thread exits are not counted twice.
        /// </summary>
        public static double GlobalNumberOfTotalRecognizedThreads
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfTotalRecognizedThreads", "# of total recognized threads", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the rate at which threads in the runtime attempt to acquire a managed lock unsuccessfully.
        /// </summary>
        public static double GlobalContentionRatePerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalContentionRatePerSecond", "Contention Rate / sec", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the total number of threads that are currently waiting to acquire a managed lock in the application. This counter is not an average over time; it displays the last observed value.
        /// </summary>
        public static double GlobalCurrentQueueLength
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalCurrentQueueLength", "Current Queue Length", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of threads per second that are waiting to acquire a lock in the application. This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double GlobalQueueLengthPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalQueueLengthPerSecond", "Queue Length / sec", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the total number of threads that waited to acquire a managed lock since the application started.
        /// </summary>
        public static double GlobalQueueLengthPeak
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalQueueLengthPeak", "Queue Length Peak", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }
        
        /// <summary>
        /// Displays the number of threads per second that have been recognized by the runtime. These threads are associated with a corresponding managed thread object. The runtime does not create these threads, but they have run inside the runtime at least once.
        /// Only unique threads are tracked; threads with the same thread ID that reenter the runtime or are recreated after the thread exits are not counted twice.
        /// This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double GlobalRateOfRecognizedThreadsPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalRateOfRecognizedThreadsPerSecond", "rate of recognized threads / sec", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the total number of times that threads in the runtime have attempted to acquire a managed lock unsuccessfully.
        /// </summary>
        public static double GlobalTotalNumberOfContentions
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalTotalNumberOfContentions", "Total # of Contentions", CategoryLocksAndThreads, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        #endregion

        #region .NET CLR Networking ("http://msdn.microsoft.com/en-us/library/70xadeyt(v=VS.100).aspx")

        /// <summary>
        /// Displays the cumulative number of bytes received over all open socket connections since the process started. This number includes data and any protocol information that is not defined by TCP/IP.
        /// </summary>
        public static double GlobalBytesReceived
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalBytesReceived", "Bytes Received", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the cumulative number of bytes sent over all open socket connections since the process started. This number includes data and any protocol information that is not defined by TCP/IP.
        /// </summary>
        public static double GlobalBytesSent
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalBytesSent", "Bytes Sent", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the cumulative number of socket connections established for this process since it started.
        /// </summary>
        public static double GlobalConnectionsEstablished
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalConnectionsEstablished", "Connections Established", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the cumulative number of datagram packets received since the process started.
        /// </summary>
        public static double GlobalDatagramsReceived
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalDatagramsReceived", "Datagrams Received", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the cumulative number of datagram packets sent since the process started.
        /// </summary>
        public static double GlobalDatagramsSent
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalDatagramsSent", "Datagrams Sent", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The average time to completion for all HttpWebRequest objects that ended in the last interval within the AppDomain since the process started.
        /// </summary>
        public static double GlobalHttpWebRequestAverageLifetime
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalHttpWebRequestAverageLifetime", "HttpWebRequest Average Lifetime", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The average time-on-queue for all HttpWebRequest objects that left the queue in the last interval within the AppDomain since the process started.
        /// </summary>
        public static double GlobalHttpWebRequestAverageQueueTime
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalHttpWebRequestAverageQueueTime", "HttpWebRequest Average Queue Time", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The number of HttpWebRequest objects created per second within the AppDomain.
        /// </summary>
        public static double GlobalHttpWebRequestsCreatedPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalHttpWebRequestsCreatedPerSecond", "HttpWebRequests Created/sec", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The number of HttpWebRequest objects that were added to the queue per second within the AppDomain.
        /// </summary>
        public static double GlobalHttpWebRequestsQueuedPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalHttpWebRequestsQueuedPerSecond", "HttpWebRequests Queued/sec", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The number of HttpWebRequest objects where the application called the Abort method per second within the AppDomain.
        /// </summary>
        public static double GlobalHttpWebRequestsAbortedPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalHttpWebRequestsAbortedPerSecond", "HttpWebRequests Aborted/sec", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The number of HttpWebRequest objects that received a failed status code from the server per second within the AppDomain.
        /// </summary>
        public static double GlobalHttpWebRequestsFailedPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalHttpWebRequestsFailedPerSecond", "HttpWebRequests Failed/sec", CategoryNetworking, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        #endregion

        #region .NET CLR Exceptions ("http://msdn.microsoft.com/en-us/library/kfhcywhs.aspx")

        /// <summary>
        /// Displays the total number of exceptions thrown since the application started. This includes both .NET exceptions and unmanaged exceptions that are converted into .NET exceptions. For example, an HRESULT returned from unmanaged code is converted to an exception in managed code.
        /// This counter includes both handled and unhandled exceptions. Exceptions that are rethrown are counted again.
        /// </summary>
        public static double GlobalNumberOfExceptionsThrown
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfExceptionsThrown", "# of Exceps Thrown", CategoryExceptions, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of exceptions thrown per second. This includes both .NET exceptions and unmanaged exceptions that are converted into .NET exceptions. For example, an HRESULT returned from unmanaged code is converted to an exception in managed code.
        /// This counter includes both handled and unhandled exceptions. It is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval. This counter is an indicator of potential performance problems if a large (>100s) number of exceptions are thrown. 
        /// </summary>
        public static double GlobalNumberOfExceptionsThrownPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfExceptionsThrownPerSecond", "# of Exceps Thrown / Sec", CategoryExceptions, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of .NET exception filters executed per second. An exception filter evaluates regardless of whether an exception is handled.
        /// This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double GlobalNumberOfExceptionFiltersPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfExceptionFiltersPerSecond", "# of Filters / Sec", CategoryExceptions, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of finally blocks executed per second. A finally block is guaranteed to be executed regardless of how the try block was exited. Only the finally blocks executed for an exception are counted; finally blocks on normal code paths are not counted by this counter.
        /// This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double GlobalNumberOfExceptionFinallysPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfExceptionFinallysPerSecond", "# of Finallys / Sec", CategoryExceptions, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of stack frames traversed, from the frame that threw the exception to the frame that handled the exception, per second. This counter resets to zero when an exception handler is entered, so nested exceptions show the handler-to-handler stack depth.
        /// This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double GlobalThrowToCatchDepthPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("GlobalNumberOfExceptionFinallysPerSecond", "Throw to Catch Depth / Sec", CategoryExceptions, GlobalInstance);
                var value = counter.NextValue();
                return value;
            }
        } 
        #endregion
        
        #endregion

        #region Process Metrics

        #region .NET CLR Memory ("http://msdn.microsoft.com/en-us/library/x2tyfybc.aspx")

        /// <summary>
        /// Displays the sum of the Gen 1 Heap Size, Gen 2 Heap Size, and the Large Object Heap Size counters. This counter indicates the current memory allocated in bytes on the garbage collection heaps.
        /// </summary>
        public static double ProcessNumberOfBytesInAllHeaps
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfBytesInAllHeaps", "# Bytes in all Heaps", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current number of garbage collection handles in use. Garbage collection handles are handles to resources external to the common language runtime and the managed environment.
        /// </summary>
        public static double ProcessNumberOfGCHandles
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfGCHandles", "# GC Handles", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of times the generation 0 objects (that is, the youngest, most recently allocated objects) are garbage collected since the application started.
        /// Generation 0 garbage collection occurs when the available memory in generation 0 is not sufficient to satisfy an allocation request. This counter is incremented at the end of a generation 0 garbage collection. Higher generation garbage collections include all lower generation collections. This counter is explicitly incremented when a higher generation (generation 1 or 2) garbage collection occurs.
        /// This counter displays the last observed value. The _Process_ counter value is not accurate and should be ignored.
        /// </summary>
        public static double ProcessNumberOfGen0Collections
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfGen0Collections", "# Gen 0 Collections", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of times the generation 1 objects are garbage collected since the application started.
        /// The counter is incremented at the end of a generation 1 garbage collection. Higher generation garbage collections include all lower generation collections. This counter is explicitly incremented when a higher generation (generation 2) garbage collection occurs.
        /// This counter displays the last observed value. The _Process_ counter value is not accurate and should be ignored.
        /// </summary>
        public static double ProcessNumberOfGen1Collections
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfGen1Collections", "# Gen 1 Collections", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of times the generation 2 objects are garbage collected since the application started. The counter is incremented at the end of a generation 2 garbage collection (also called a full garbage collection).
        /// This counter displays the last observed value. The _Process_ counter value is not accurate and should be ignored.
        /// </summary>
        public static double ProcessNumberOfGen2Collections
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfGen2Collections", "# Gen 2 Collections", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the peak number of times garbage collection was performed because of an explicit call to GC.Collect. It is good practice to let the garbage collector tune the frequency of its collections.
        /// </summary>
        public static double ProcessNumberOfInducedGC
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfInducedGC", "# Induced GC", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of pinned objects encountered in the last garbage collection. A pinned object is one that the garbage collector cannot move in memory. This counter tracks the pinned objects only in the heaps that are garbage collected. For example, a generation 0 garbage collection causes enumeration of pinned objects only in the generation 0 heap.
        /// </summary>
        public static double ProcessNumberOfPinnedObjects
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfPinnedObjects", "# of Pinned Objects", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current number of syncronization blocks in use. Synchronization blocks are per-object data structures allocated for storing synchronization information. Synchronization blocks hold weak references to managed objects and must be scanned by the garbage collector. Synchronization blocks are not limited to storing synchronization information; they can also store COM interop metadata. This counter indicates performance problems with heavy use of synchronization primitives.
        /// </summary>
        public static double ProcessNumberOfSinkBlocksInUse
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfSinkBlocksInUse", "# of Sink Blocks in use", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the amount of virtual memory, in bytes, currently committed by the garbage collector. Committed memory is the physical memory for which space has been reserved in the disk paging file.
        /// </summary>
        public static double ProcessNumberOfTotalCommittedBytes
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfTotalCommittedBytes", "# Total committed Bytes", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the amount of virtual memory. in bytes, currently reserved by the garbage collector. Reserved memory is the virtual memory space reserved for the application but no disk or main memory pages have been used.
        /// </summary>
        public static double ProcessNumberOfTotalReservedBytes
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfTotalReservedBytes", "# Total reserved Bytes", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the percentage of elapsed time that was spent performing a garbage collection since the last garbage collection cycle. This counter usually indicates the work done by the garbage collector to collect and compact memory on behalf of the application. This counter is updated only at the end of every garbage collection. This counter is not an average; its value reflects the last observed value.
        /// </summary>
        public static double ProcessPercentTimeInGC
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessPercentTimeInGC", "% Time in GC", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of bytes per second allocated on the garbage collection heap. This counter is updated at the end of every garbage collection, not at each allocation. This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double ProcessAllocatedBytesPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessAllocatedBytesPerSecond", "Allocated Bytes/second", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of garbage-collected objects that survive a collection because they are waiting to be finalized. If these objects hold references to other objects, those objects also survive but are not counted by this counter. The Promoted Finalization-Memory from Gen 0 and Promoted Finalization-Memory from Gen 1 counters represent all the memory that survived due to finalization.    
        /// This counter is not cumulative; it is updated at the end of every garbage collection with the count of the survivors during that particular collection only. This counter indicates the extra overhead that the application might incur because of finalization.
        /// </summary>
        public static double ProcessFinalizationSurvivors
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessFinalizationSurvivors", "Finalization Survivors", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the maximum bytes that can be allocated in generation 0; it does not indicate the current number of bytes allocated in generation 0.
        /// A generation 0 garbage collection occurs when the allocations since the last collection exceed this size. The generation 0 size is tuned by the garbage collector and can change during the execution of the application. At the end of a generation 0 collection the size of the generation 0 heap is 0 bytes. This counter displays the size, in bytes, of allocations that invokes the next generation 0 garbage collection.
        /// This counter is updated at the end of a garbage collection, not at each allocation.
        /// </summary>
        public static double ProcessGen0HeapSize
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessGen0HeapSize", "Gen 0 heap size", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes per second that are promoted from generation 0 to generation 1. Memory is promoted when it survives a garbage collection. This counter is an indicator of relatively long-lived objects being created per second.
        /// This counter displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double ProcessGen0PromotedBytesPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessGen0PromotedBytesPerSecond", "Gen 0 Promoted Bytes/Sec", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current number of bytes in generation 1; this counter does not display the maximum size of generation 1. Objects are not directly allocated in this generation; they are promoted from previous generation 0 garbage collections. This counter is updated at the end of a garbage collection, not at each allocation.
        /// </summary>
        public static double ProcessGen1HeapSize
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessGen1HeapSize", "Gen 1 heap size", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes per second that are promoted from generation 1 to generation 2. Objects that are promoted only because they are waiting to be finalized are not included in this counter.
        /// Memory is promoted when it survives a garbage collection. Nothing is promoted from generation 2 because it is the oldest generation. This counter is an indicator of very long-lived objects being created per second.
        /// This counter displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double ProcessGen1PromotedBytesPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessGen1PromotedBytesPerSecond", "Gen 1 Promoted Bytes/Sec", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current number of bytes in generation 2. Objects are not directly allocated in this generation; they are promoted from generation 1 during previous generation 1 garbage collections. This counter is updated at the end of a garbage collection, not at each allocation.
        /// </summary>
        public static double ProcessGen2HeapSize
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessGen2HeapSize", "Gen 2 heap size", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the current size, in bytes, of the Large Object Heap. Objects that are greater than approximately 85,000 bytes are treated as large objects by the garbage collector and are directly allocated in a special heap; they are not promoted through the generations. This counter is updated at the end of a garbage collection, not at each allocation.
        /// </summary>
        public static double ProcessLargeObjectHeapSize
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessLargeObjectHeapSize", "Large Object Heap size", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the process ID of the CLR process instance that is being monitored.
        /// </summary>
        public static double ProcessId
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessId", "Process ID", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes of memory that are promoted from generation 0 to generation 1 only because they are waiting to be finalized. This counter is not cumulative; it displays the value observed at the end of the last garbage collection.
        /// </summary>
        public static double ProcessPromotedFinalizationMemoryFromGen0
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessPromotedFinalizationMemoryFromGen0", "Promoted Finalization-Memory from Gen 0", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes of memory that survive garbage collection and are promoted from generation 0 to generation 1. Objects that are promoted only because they are waiting to be finalized are not included in this counter. This counter is not cumulative; it displays the value observed at the end of the last garbage collection.
        /// </summary>
        public static double ProcessPromotedMemoryFromGen0
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessPromotedMemoryFromGen0", "Promoted Memory from Gen 0", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the bytes of memory that survive garbage collection and are promoted from generation 1 to generation 2. Objects that are promoted only because they are waiting to be finalized are not included in this counter. This counter is not cumulative; it displays the value observed at the end of the last garbage collection. This counter is reset to 0 if the last garbage collection was a generation 0 collection only.
        /// </summary>
        public static double ProcessPromotedMemoryFromGen1
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessPromotedMemoryFromGen1", "Promoted Memory from Gen 1", CategoryMemory, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        #endregion
        
        #region .NET CLR LocksAndThreads ("http://msdn.microsoft.com/en-us/library/zf749bat(v=VS.71).aspx")

        /// <summary>
        /// Displays the number of current managed thread objects in the application. This counter maintains the count of both running and stopped threads. This counter is not an average over time; it displays only the last observed value.
        /// </summary>
        public static double ProcessNumberOfCurrentLogicalThreads
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfCurrentLogicalThreads", "# of current logical Threads", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of native operating system threads created and owned by the common language runtime to act as underlying threads for managed thread objects. This counter's value does not include the threads used by the runtime in its internal operations; it is a subset of the threads in the operating system process.
        /// </summary>
        public static double ProcessNumberOfCurrentPhysicalThreads
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfCurrentPhysicalThreads", "# of current physical Threads", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of threads that are currently recognized by the runtime. These threads are associated with a corresponding managed thread object. The runtime does not create these threads, but they have run inside the runtime at least once.
        /// Only unique threads are tracked; threads with the same thread ID that reenter the runtime or are recreated after the thread exits are not counted twice.
        /// </summary>
        public static double ProcessNumberOfCurrentRecognizedThreads
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfCurrentRecognizedThreads", "# of current recognized threads", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the total number of threads that have been recognized by the runtime since the application started. These threads are associated with a corresponding managed thread object. The runtime does not create these threads, but they have run inside the runtime at least once.
        /// Only unique threads are tracked; threads with the same thread ID that reenter the runtime or are recreated after the thread exits are not counted twice.
        /// </summary>
        public static double ProcessNumberOfTotalRecognizedThreads
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfTotalRecognizedThreads", "# of total recognized threads", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the rate at which threads in the runtime attempt to acquire a managed lock unsuccessfully.
        /// </summary>
        public static double ProcessContentionRatePerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessContentionRatePerSecond", "Contention Rate / sec", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the total number of threads that are currently waiting to acquire a managed lock in the application. This counter is not an average over time; it displays the last observed value.
        /// </summary>
        public static double ProcessCurrentQueueLength
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessCurrentQueueLength", "Current Queue Length", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of threads per second that are waiting to acquire a lock in the application. This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double ProcessQueueLengthPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessQueueLengthPerSecond", "Queue Length / sec", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the total number of threads that waited to acquire a managed lock since the application started.
        /// </summary>
        public static double ProcessQueueLengthPeak
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessQueueLengthPeak", "Queue Length Peak", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of threads per second that have been recognized by the runtime. These threads are associated with a corresponding managed thread object. The runtime does not create these threads, but they have run inside the runtime at least once.
        /// Only unique threads are tracked; threads with the same thread ID that reenter the runtime or are recreated after the thread exits are not counted twice.
        /// This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double ProcessRateOfRecognizedThreadsPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessRateOfRecognizedThreadsPerSecond", "rate of recognized threads / sec", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the total number of times that threads in the runtime have attempted to acquire a managed lock unsuccessfully.
        /// </summary>
        public static double ProcessTotalNumberOfContentions
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessTotalNumberOfContentions", "Total # of Contentions", CategoryLocksAndThreads, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        #endregion

        #region .NET CLR Networking ("http://msdn.microsoft.com/en-us/library/70xadeyt(v=VS.100).aspx")

        /// <summary>
        /// Displays the cumulative number of bytes received over all open socket connections since the process started. This number includes data and any protocol information that is not defined by TCP/IP.
        /// </summary>
        public static double ProcessBytesReceived
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessBytesReceived", "Bytes Received", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the cumulative number of bytes sent over all open socket connections since the process started. This number includes data and any protocol information that is not defined by TCP/IP.
        /// </summary>
        public static double ProcessBytesSent
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessBytesSent", "Bytes Sent", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the cumulative number of socket connections established for this process since it started.
        /// </summary>
        public static double ProcessConnectionsEstablished
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessConnectionsEstablished", "Connections Established", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the cumulative number of datagram packets received since the process started.
        /// </summary>
        public static double ProcessDatagramsReceived
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessDatagramsReceived", "Datagrams Received", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the cumulative number of datagram packets sent since the process started.
        /// </summary>
        public static double ProcessDatagramsSent
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessDatagramsSent", "Datagrams Sent", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The average time to completion for all HttpWebRequest objects that ended in the last interval within the AppDomain since the process started.
        /// </summary>
        public static double ProcessHttpWebRequestAverageLifetime
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessHttpWebRequestAverageLifetime", "HttpWebRequest Average Lifetime", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The average time-on-queue for all HttpWebRequest objects that left the queue in the last interval within the AppDomain since the process started.
        /// </summary>
        public static double ProcessHttpWebRequestAverageQueueTime
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessHttpWebRequestAverageQueueTime", "HttpWebRequest Average Queue Time", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The number of HttpWebRequest objects created per second within the AppDomain.
        /// </summary>
        public static double ProcessHttpWebRequestsCreatedPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessHttpWebRequestsCreatedPerSecond", "HttpWebRequests Created/sec", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The number of HttpWebRequest objects that were added to the queue per second within the AppDomain.
        /// </summary>
        public static double ProcessHttpWebRequestsQueuedPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessHttpWebRequestsQueuedPerSecond", "HttpWebRequests Queued/sec", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The number of HttpWebRequest objects where the application called the Abort method per second within the AppDomain.
        /// </summary>
        public static double ProcessHttpWebRequestsAbortedPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessHttpWebRequestsAbortedPerSecond", "HttpWebRequests Aborted/sec", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// The number of HttpWebRequest objects that received a failed status code from the server per second within the AppDomain.
        /// </summary>
        public static double ProcessHttpWebRequestsFailedPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessHttpWebRequestsFailedPerSecond", "HttpWebRequests Failed/sec", CategoryNetworking, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        #endregion

        #region .NET CLR Exceptions ("http://msdn.microsoft.com/en-us/library/kfhcywhs.aspx")

        /// <summary>
        /// Displays the total number of exceptions thrown since the application started. This includes both .NET exceptions and unmanaged exceptions that are converted into .NET exceptions. For example, an HRESULT returned from unmanaged code is converted to an exception in managed code.
        /// This counter includes both handled and unhandled exceptions. Exceptions that are rethrown are counted again.
        /// </summary>
        public static double ProcessNumberOfExceptionsThrown
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfExceptionsThrown", "# of Exceps Thrown", CategoryExceptions, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of exceptions thrown per second. This includes both .NET exceptions and unmanaged exceptions that are converted into .NET exceptions. For example, an HRESULT returned from unmanaged code is converted to an exception in managed code.
        /// This counter includes both handled and unhandled exceptions. It is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval. This counter is an indicator of potential performance problems if a large (>100s) number of exceptions are thrown. 
        /// </summary>
        public static double ProcessNumberOfExceptionsThrownPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfExceptionsThrownPerSecond", "# of Exceps Thrown / Sec", CategoryExceptions, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of .NET exception filters executed per second. An exception filter evaluates regardless of whether an exception is handled.
        /// This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double ProcessNumberOfExceptionFiltersPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfExceptionFiltersPerSecond", "# of Filters / Sec", CategoryExceptions, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of finally blocks executed per second. A finally block is guaranteed to be executed regardless of how the try block was exited. Only the finally blocks executed for an exception are counted; finally blocks on normal code paths are not counted by this counter.
        /// This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double ProcessNumberOfExceptionFinallysPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfExceptionFinallysPerSecond", "# of Finallys / Sec", CategoryExceptions, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        /// <summary>
        /// Displays the number of stack frames traversed, from the frame that threw the exception to the frame that handled the exception, per second. This counter resets to zero when an exception handler is entered, so nested exceptions show the handler-to-handler stack depth.
        /// This counter is not an average over time; it displays the difference between the values observed in the last two samples divided by the duration of the sample interval.
        /// </summary>
        public static double ProcessThrowToCatchDepthPerSecond
        {
            get
            {
                var counter = GetOrInstallCounter("ProcessNumberOfExceptionFinallysPerSecond", "Throw to Catch Depth / Sec", CategoryExceptions, Process);
                var value = counter.NextValue();
                return value;
            }
        }

        #endregion
        
        #endregion
        
        private static PerformanceCounter GetOrInstallCounter(string property, string name, string category, string instance = null)
        {
            if (!Counters[Process].ContainsKey(property))
            {
                var counter = new PerformanceCounter(category, name, instance ?? Process, true);

                Counters[Process].Add(property, counter);
            }
            return Counters[Process][property];
        }
    }
}