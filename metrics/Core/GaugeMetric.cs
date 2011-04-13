using System;
using Newtonsoft.Json;

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
    public sealed class GaugeMetric<T> : IMetric
    {
        private readonly Func<T> _evaluator;

        [JsonIgnore]
        public IMetric Copy
        {
            get { return new GaugeMetric<T>(_evaluator); }
        }

        public GaugeMetric(Func<T> evaluator)
        {
            _evaluator = evaluator;
        }

        public T Value
        {
            get { return _evaluator.Invoke(); }
        }
    }
}