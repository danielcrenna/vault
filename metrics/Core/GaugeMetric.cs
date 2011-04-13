using System;

namespace metrics.Core
{
    /// <summary>
    /// A gauge metric is an instantaneous reading of a partiular value. To
    /// instrument a queue's depth, for example:
    /// <example>
    /// <code>
    /// var queue = new Queue<int>();
    /// var gauge = new GaugeMetric<int>(() => queue.Count);
    /// </code>
    /// </example>
    /// </summary>
    public class GaugeMetric<T> : IMetric
    {
        private readonly Func<T> _evaluator;

        public virtual IMetric Copy
        {
            get { return new GaugeMetric<T>(_evaluator); }
        }

        public GaugeMetric(Func<T> evaluator)
        {
            _evaluator = evaluator;
        }

        public virtual T Value
        {
            get { return _evaluator.Invoke(); }
        }
    }
}