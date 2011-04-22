using System.Threading;

namespace metrics.Support
{
    /// <summary>
    /// Provides support for atomic operations around a <see cref="long" /> value
    /// </summary>
    public struct AtomicLong
    {
        private long _value;

        public AtomicLong(long value) : this()
        {
            Set(value);
        }

        /// <summary>
        /// Get the current value
        /// </summary>
        public long Get()
        {
            return Interlocked.Read(ref _value);    
        }

        /// <summary>
        /// Set to the given value
        /// </summary>
        public void Set(long value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        /// <summary>
        /// Atomically add the given value to the current value
        /// </summary>
        public long AddAndGet(long amount)
        {
            Interlocked.Add(ref _value, amount);
            return Get();
        }

        public static implicit operator AtomicLong(long value)
        {
            return new AtomicLong(value);
        }

        public static implicit operator long(AtomicLong value)
        {
            return value.Get();
        }

        /// <summary>
        /// Set to the given value and return the previous value
        /// </summary>
        public long GetAndSet(long value)
        {
            var previous = Get();
            Set(value);
            return previous;
        }
    }
}