using System;

namespace ab
{
    public class MetricEventArgs : EventArgs
    {
        public string Name { get; set; }
        public long Timestamp { get; set; }
        public long Value { get; set; }

        public MetricEventArgs(string name, long timestamp, long value)
        {
            Name = name;
            Timestamp = timestamp;
            Value = value;
        }
    }
}