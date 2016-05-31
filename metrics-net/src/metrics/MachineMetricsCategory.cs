using System;

namespace metrics
{
    [Flags]
    public enum MachineMetricsCategory : short
    {
        None = 0x0,
        PhysicalDisk = 0x1,
        LogicalDisk = 0x2,
        LocksAndThreads = 0x4,
        Memory = 0x8,
        All = short.MaxValue
    }
}