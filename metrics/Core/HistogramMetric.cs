using System;

namespace metrics.Core
{
    public class HistogramMetric : IMetric
    {
        public IMetric Copy
        {
            get { throw new NotImplementedException(); }
        }

        public double Min { get; set; }
        public double Max { get; set; }
        public double Mean { get; set; }
        public double StdDev { get; set; }

        public double[] Percentiles(params double[] args)
        {
            throw new NotImplementedException();
        }
    }
}
