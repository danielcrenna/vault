using System.Threading;

namespace metrics.Core
{
    /// <summary>
    /// An atomic counter metric
    /// </summary>
    public class CounterMetric : IMetric
    {
        private long _count;

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

        public IMetric Copy
        {
            get { return new CounterMetric(_count); }
        }
    }
}