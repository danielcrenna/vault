using System;
using System.Collections.Generic;

namespace metrics.Stats
{

    /// <summary>
    /// An exponentially-decaying random sample of {@code long}s. Uses Cormode et
    /// al's forward-decaying priority reservoir sampling method to produce a
    /// statistically representative sample, exponentially biased towards newer
    /// entries.
    /// </summary>
    /// <see href="http://www.research.att.com/people/Cormode_Graham/library/publications/CormodeShkapenyukSrivastavaXu09.pdf" />
    public class ExponentiallyDecayingSample : ISample
    {
        public void Clear()
        {
            throw new NotImplementedException();
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