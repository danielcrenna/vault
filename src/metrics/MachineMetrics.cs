namespace metrics
{
    /// <summary>
    /// A convenience class for installing machine-level metrics
    /// <seealso href="http://technet.microsoft.com/en-us/library/cc768048.aspx#XSLTsection132121120120" />
    /// </summary>
    public static class MachineMetrics
    {
        public static void InstallAll()
        {
            InstallPhysicalDisk();
            InstallLogicalDisk();

            //Metrics.GetOrAdd(new MetricName(typeof(Metrics), Environment.MachineName + ".clr.locks_and_threads.machine_total_contentions"), new GaugeMetric<double>(() => CLRProfiler.MachineTotalContentions));
            //Metrics.GetOrAdd(new MetricName(typeof(Metrics), Environment.MachineName + ".clr.locks_and_threads.machine_contention_rate_per_second"), new GaugeMetric<double>(() => CLRProfiler.MachineContentionRatePerSecond));
            //Metrics.GetOrAdd(new MetricName(typeof(Metrics), Environment.MachineName + ".clr.locks_and_threads.machine_current_queue_length"), new GaugeMetric<double>(() => CLRProfiler.MachineCurrentQueueLength));
            //Metrics.GetOrAdd(new MetricName(typeof(Metrics), Environment.MachineName + ".clr.locks_and_threads.machine_queue_length_peak"), new GaugeMetric<double>(() => CLRProfiler.MachineQueueLengthPeak));
        }

        public static void InstallPhysicalDisk()
        {
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Current Disk Queue Length", "_Total", ".physical_disk.current_disk_queue_length");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Queue Length", "_Total", ".physical_disk.avg_disk_queue_length");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Read Queue Length", "_Total", ".physical_disk.disk_read_queue_length");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Write Queue Length", "_Total", ".physical_disk.disk_write_queue_length");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "% Disk Time", "_Total", ".physical_disk.percent_disk_time");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "% Disk Read Time", "_Total", ".physical_disk.percent_disk_read_time");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "% Disk Write Time", "_Total", ".physical_disk.percent_disk_write_time");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk sec/Transfer", "_Total", ".physical_disk.avg_disk_seconds_per_transfer");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk sec/Read", "_Total", ".physical_disk.avg_disk_seconds_per_read");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk sec/Write", "_Total", ".physical_disk.avg_disk_seconds_per_write");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Transfers/sec", "_Total", ".physical_disk.disk_transfers_per_second");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Reads/sec", "_Total", ".physical_disk.disk_reads_per_second");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Writes/sec", "_Total", ".physical_disk.disk_writes_per_second");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Bytes/sec", "_Total", ".physical_disk.disk_bytes_per_second");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Read Bytes/sec", "_Total", ".physical_disk.disk_read_bytes_per_second");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Disk Write Bytes/sec", "_Total", ".physical_disk.disk_write_bytes_per_second");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Bytes/Transfer", "_Total", ".physical_disk.avg_disk_bytes_per_transfer");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Bytes/Read", "_Total", ".physical_disk.avg_disk_bytes_per_read");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Avg. Disk Bytes/Write", "_Total", ".physical_disk.avg_disk_bytes_per_write");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "% Idle Time", "_Total", ".physical_disk.percent_idle_time");
            Metrics.InstallPerformanceCounterGauge("PhysicalDisk", "Split IO/Sec", "_Total", ".physical_disk.split_io_per_second");
        }

        public static void InstallLogicalDisk()
        {
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Current Disk Queue Length", "_Total", ".logical_disk.current_disk_queue_length");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Queue Length", "_Total", ".logical_disk.avg_disk_queue_length");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Read Queue Length", "_Total", ".logical_disk.disk_read_queue_length");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Write Queue Length", "_Total", ".logical_disk.disk_write_queue_length");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "% Disk Time", "_Total", ".logical_disk.percent_disk_time");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "% Disk Read Time", "_Total", ".logical_disk.percent_disk_read_time");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "% Disk Write Time", "_Total", ".logical_disk.percent_disk_write_time");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk sec/Transfer", "_Total", ".logical_disk.avg_disk_seconds_per_transfer");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk sec/Read", "_Total", ".logical_disk.avg_disk_seconds_per_read");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk sec/Write", "_Total", ".logical_disk.avg_disk_seconds_per_write");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Transfers/sec", "_Total", ".logical_disk.disk_transfers_per_second");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Reads/sec", "_Total", ".logical_disk.disk_reads_per_second");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Writes/sec", "_Total", ".logical_disk.disk_writes_per_second");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Bytes/sec", "_Total", ".logical_disk.disk_bytes_per_second");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Read Bytes/sec", "_Total", ".logical_disk.disk_read_bytes_per_second");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Disk Write Bytes/sec", "_Total", ".logical_disk.disk_write_bytes_per_second");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Bytes/Transfer", "_Total", ".logical_disk.avg_disk_bytes_per_transfer");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Bytes/Read", "_Total", ".logical_disk.avg_disk_bytes_per_read");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Avg. Disk Bytes/Write", "_Total", ".logical_disk.avg_disk_bytes_per_write");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "% Idle Time", "_Total", ".logical_disk.percent_idle_time");
            Metrics.InstallPerformanceCounterGauge("LogicalDisk", "Split IO/Sec", "_Total", ".logical_disk.split_io_per_second");
        }
    }
}