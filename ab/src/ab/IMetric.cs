using System;

namespace ab
{
    public interface IMetric
    {
        string Name { get; }
        double[] Values(DateTime start, DateTime end);
        string Description { get; }
        double[] Bounds { get; }
        event EventHandler<MetricEventArgs> Hook;
        void OnHook(MetricEventArgs e);
    }
}