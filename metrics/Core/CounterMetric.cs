using System.Threading;
using Newtonsoft.Json;

namespace metrics.Core
{
    /// <summary>
    /// An atomic counter metric
    /// </summary>
    public sealed class CounterMetric : IMetric
    {
        private /* atomic */ long _count;

        public CounterMetric()
        {
            
        }

        private CounterMetric(long count)
        {
            Interlocked.Exchange(ref _count, count);
        }

        public void Increment()
        {
            Interlocked.Increment(ref _count);
        }

        public void Increment(long amount)
        {
            Interlocked.Add(ref _count, amount);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref _count);
        }

        public void Decrement(long amount)
        {
            Increment(-amount);
        }

        public void Clear()
        {
            Interlocked.Exchange(ref _count, 0);
        }

        public long Count
        {
            get { return Interlocked.Read(ref _count); }
        }

        [JsonIgnore]
        public IMetric Copy
        {
            get { return new CounterMetric(_count); }
        }
    }
}