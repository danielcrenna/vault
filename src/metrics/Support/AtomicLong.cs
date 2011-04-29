using System.Threading;

namespace metrics.Support
{
    /// <summary>
    /// Provides support for atomic operations around a <see cref="long" /> value
    /// </summary>
    internal class AtomicLong
    {
        private long _value;

        public AtomicLong()
        {
            Set(0);
        }

        public AtomicLong(long value)
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

        /// <summary>
        /// Atomically increments by one and returns the current value
        /// </summary>
        /// <returns></returns>
        public long IncrementAndGet()
        {
            Interlocked.Increment(ref _value);
            return Get();
        }

        /// <summary>
        /// Atomically set the value to the given updated value if the current value == expected value
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="updated">The new value</param>
        /// <returns></returns>
        public bool CompareAndSet(long expected, long updated)
        {
            if(Get() == expected)
            {
                Set(updated);
                return true;
            }
            return false;
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

        /// <summary>
        /// Adds the given value and return the previous value
        /// </summary>
        public long GetAndAdd(long value)
        {
            var previous = Get();
            Interlocked.Add(ref _value, value);
            return previous;
        }

        public static implicit operator AtomicLong(long value)
        {
            return new AtomicLong(value);
        }

        public static implicit operator long(AtomicLong value)
        {
            return value.Get();
        }
    }
}