using System;

namespace ab
{
    public abstract class MetricBase : IMetric
    {
        public virtual string Name { get; set; }
        public abstract double[] Values(DateTime start, DateTime end);
        public virtual string Description { get; set; }
        public virtual double[] Bounds { get; set; }
        public virtual event EventHandler<MetricEventArgs> Hook;

        public virtual void OnHook(MetricEventArgs e)
        {
            var handler = Hook;
            if (handler == null) return;
            handler(this, e);
        }
    }
}