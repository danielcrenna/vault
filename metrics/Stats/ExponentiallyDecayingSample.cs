using System;
using System.Collections.Generic;
using metrics.Support;

namespace metrics.Stats
{
    /// <summary>
    /// An exponentially-decaying random sample of {@code long}s. Uses Cormode et
    /// al's forward-decaying priority reservoir sampling method to produce a
    /// statistically representative sample, exponentially biased towards newer
    /// entries.
    /// </summary>
    /// <see href="http://www.research.att.com/people/Cormode_Graham/library/publications/CormodeShkapenyukSrivastavaXu09.pdf">
    /// Cormode et al. Forward Decay: A Practical Time Decay Model for Streaming
    /// Systems. ICDE '09: Proceedings of the 2009 IEEE International Conference on
    /// Data Engineering (2009)
    /// </see>
    public class ExponentiallyDecayingSample : ISample
    {
        private readonly int _reservoirSize;
        private readonly double _alpha;
        private AtomicLong _count = new AtomicLong(0);
        //private volatile long startTime;
        private /* atomic */ long nextScaleTime;

        /// <param name="reservoirSize">The number of samples to keep in the sampling reservoir</param>
        /// <param name="alpha">The exponential decay factor; the higher this is, the more biased the sample will be towards newer values</param>
        public ExponentiallyDecayingSample(int reservoirSize, double alpha)
        {
            _reservoirSize = reservoirSize;
            _alpha = alpha;
        }

        public void Clear()
        {
            
        }

        public int Size
        {
            get { throw new NotImplementedException(); }
        }

        public void Update(long value)
        {
            throw new NotImplementedException();
        }

        public ICollection<long> Values
        {
            get { throw new NotImplementedException(); }
        }
    }
}