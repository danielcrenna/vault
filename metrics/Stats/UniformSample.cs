using System;
using System.Collections.Generic;
using System.Threading;

namespace metrics.Stats
{
    public class UniformSample : ISample
    {
        private /* atomic */ long _count;
        private /* atomic */ readonly long[] _values;
        
        public UniformSample(int reservoirSize)
        {
            _values = new long[reservoirSize];
            Clear();
        }

        /// <summary>
        /// Clears all recorded values
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < _values.Length; i++)
            {
                Interlocked.Exchange(ref _values[i], 0);
            }
            Interlocked.Exchange(ref _count, 0);
        }

        /// <summary>
        /// Returns the number of values recorded
        /// </summary>
        public int Size
        {
            get
            {
                var c = Interlocked.Read(ref _count);
                if (c > _values.Length)
                {
                    return _values.Length;
                }
                return (int) c;
            }
        }

        /// <summary>
        /// Adds a new recorded value to the sample
        /// </summary>
        public void Update(long value)
        {
            var count = Interlocked.Add(ref _count, 1);
            if (count <= _values.Length)
            {
                var index = (int) count - 1;
                Interlocked.Exchange(ref _values[index], value);
            }
            else
            {
                var random = Math.Abs(Support.Random.NextLong()) % count;
                if (random < _values.Length)
                {
                    var index = (int) random;
                    Interlocked.Exchange(ref _values[index], value);
                }
            }
        }
        
        /// <summary>
        /// Returns a copy of the sample's values
        /// </summary>
        public ICollection<long> Values
        {
            get
            {
                var size = Size;
                var copy = new List<long>(size);
                for (var i = 0; i < size; i++)
                {
                    copy.Add(Interlocked.Read(ref _values[i]));
                }
                return copy;       
            }
        }
    }
}
