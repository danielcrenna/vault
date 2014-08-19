using System;

namespace metrics
{
    /// <summary>
    /// A convenience class for installing global, machine-level metrics
    /// <seealso href="http://technet.microsoft.com/en-us/library/cc768048.aspx#XSLTsection132121120120" />
    /// <seealso href="http://msdn.microsoft.com/en-us/library/w8f5kw2e%28v=VS.71%29.aspx" />
    /// </summary>
    public  class MachineMetrics
    {
        private readonly Metrics _metrics;
        private const string TotalInstance = "_Total";
        private const string GlobalInstance = "_Global_";

        public MachineMetrics(Metrics metrics)
        {
            _metrics = metrics;
        }

        public  void InstallAll()
        {
            InstallPhysicalDisk();
            InstallLogicalDisk();
        }

        public  void InstallPhysicalDisk()
        {
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Current Disk Queue Length", TotalInstance, ".physical_disk.current_disk_queue_length");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Queue Length", TotalInstance, ".physical_disk.avg_disk_queue_length");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Read Queue Length", TotalInstance, ".physical_disk.disk_read_queue_length");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Write Queue Length", TotalInstance, ".physical_disk.disk_write_queue_length");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "% Disk Time", TotalInstance, ".physical_disk.percent_disk_time");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "% Disk Read Time", TotalInstance, ".physical_disk.percent_disk_read_time");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "% Disk Write Time", TotalInstance, ".physical_disk.percent_disk_write_time");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk sec/Transfer", TotalInstance, ".physical_disk.avg_disk_seconds_per_transfer");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk sec/Read", TotalInstance, ".physical_disk.avg_disk_seconds_per_read");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk sec/Write", TotalInstance, ".physical_disk.avg_disk_seconds_per_write");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Transfers/sec", TotalInstance, ".physical_disk.disk_transfers_per_second");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Reads/sec", TotalInstance, ".physical_disk.disk_reads_per_second");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Writes/sec", TotalInstance, ".physical_disk.disk_writes_per_second");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Bytes/sec", TotalInstance, ".physical_disk.disk_bytes_per_second");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Read Bytes/sec", TotalInstance, ".physical_disk.disk_read_bytes_per_second");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Write Bytes/sec", TotalInstance, ".physical_disk.disk_write_bytes_per_second");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Bytes/Transfer", TotalInstance, ".physical_disk.avg_disk_bytes_per_transfer");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Bytes/Read", TotalInstance, ".physical_disk.avg_disk_bytes_per_read");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Bytes/Write", TotalInstance, ".physical_disk.avg_disk_bytes_per_write");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "% Idle Time", TotalInstance, ".physical_disk.percent_idle_time");
            _metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Split IO/Sec", TotalInstance, ".physical_disk.split_io_per_second");
        }

        public  void InstallLogicalDisk()
        {
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Current Disk Queue Length", TotalInstance, ".logical_disk.current_disk_queue_length");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Queue Length", TotalInstance, ".logical_disk.avg_disk_queue_length");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Read Queue Length", TotalInstance, ".logical_disk.disk_read_queue_length");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Write Queue Length", TotalInstance, ".logical_disk.disk_write_queue_length");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "% Disk Time", TotalInstance, ".logical_disk.percent_disk_time");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "% Disk Read Time", TotalInstance, ".logical_disk.percent_disk_read_time");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "% Disk Write Time", TotalInstance, ".logical_disk.percent_disk_write_time");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk sec/Transfer", TotalInstance, ".logical_disk.avg_disk_seconds_per_transfer");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk sec/Read", TotalInstance, ".logical_disk.avg_disk_seconds_per_read");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk sec/Write", TotalInstance, ".logical_disk.avg_disk_seconds_per_write");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Transfers/sec", TotalInstance, ".logical_disk.disk_transfers_per_second");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Reads/sec", TotalInstance, ".logical_disk.disk_reads_per_second");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Writes/sec", TotalInstance, ".logical_disk.disk_writes_per_second");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Bytes/sec", TotalInstance, ".logical_disk.disk_bytes_per_second");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Read Bytes/sec", TotalInstance, ".logical_disk.disk_read_bytes_per_second");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Write Bytes/sec", TotalInstance, ".logical_disk.disk_write_bytes_per_second");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Bytes/Transfer", TotalInstance, ".logical_disk.avg_disk_bytes_per_transfer");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Bytes/Read", TotalInstance, ".logical_disk.avg_disk_bytes_per_read");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Bytes/Write", TotalInstance, ".logical_disk.avg_disk_bytes_per_write");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "% Idle Time", TotalInstance, ".logical_disk.percent_idle_time");
            _metrics.InstallPerformanceCounterGauge("LogicalDisk", "Split IO/Sec", TotalInstance, ".logical_disk.split_io_per_second");
        }
        
        public  void InstallCLRLocksAndThreads()
        {
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "Total # of Contentions", GlobalInstance, ".clr_locks_and_threads.total_number_of_contentions");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "Contention Rate / sec", GlobalInstance, ".clr_locks_and_threads.contention_rate_per_second");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "Current Queue Length", GlobalInstance, ".clr_locks_and_threads.current_queue_length");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "Queue Length Peak", GlobalInstance, ".clr_locks_and_threads.queue_length_peak");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "Queue Length / sec", GlobalInstance, ".clr_locks_and_threads.queue_length_per_second");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "# of current logical Threads", GlobalInstance, ".clr_locks_and_threads.number_of_current_logical_threads");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "# of current physical Threads", GlobalInstance, ".clr_locks_and_threads.number_of_current_physical_threads");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "# of current recognized threads", GlobalInstance, ".clr_locks_and_threads.number_of_current_recognized_threads");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "# of total recognized threads", GlobalInstance, ".clr_locks_and_threads.number_of_total_recognized_threads");
            _metrics.InstallPerformanceCounterGauge(".NET CLR LocksAndThreads", "rate of recognized threads / sec", GlobalInstance, ".clr_locks_and_threads.rate_or_recognized_threads_per_second");
        }

        //_Global_:.NET CLR Memory:# Gen 0 Collections
        //_Global_:.NET CLR Memory:# Gen 1 Collections
        //_Global_:.NET CLR Memory:# Gen 2 Collections
        //_Global_:.NET CLR Memory:Promoted Memory from Gen 0
        //_Global_:.NET CLR Memory:Promoted Memory from Gen 1
        //_Global_:.NET CLR Memory:Gen 0 Promoted Bytes/Sec
        //_Global_:.NET CLR Memory:Gen 1 Promoted Bytes/Sec
        //_Global_:.NET CLR Memory:Promoted Finalization-Memory from Gen 0
        //_Global_:.NET CLR Memory:Process ID
        //_Global_:.NET CLR Memory:Gen 0 heap size
        //_Global_:.NET CLR Memory:Gen 1 heap size
        //_Global_:.NET CLR Memory:Gen 2 heap size
        //_Global_:.NET CLR Memory:Large Object Heap size
        //_Global_:.NET CLR Memory:Finalization Survivors
        //_Global_:.NET CLR Memory:# GC Handles
        //_Global_:.NET CLR Memory:Allocated Bytes/sec
        //_Global_:.NET CLR Memory:# Induced GC
        //_Global_:.NET CLR Memory:% Time in GC
        //_Global_:.NET CLR Memory:Not Displayed
        //_Global_:.NET CLR Memory:# Bytes in all Heaps
        //_Global_:.NET CLR Memory:# Total committed Bytes
        //_Global_:.NET CLR Memory:# Total reserved Bytes
        //_Global_:.NET CLR Memory:# of Pinned Objects
        //_Global_:.NET CLR Memory:# of Sink Blocks in use
        public  void InstallCLRMemory()
        {
            throw new NotImplementedException();
        }
    }
}