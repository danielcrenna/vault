using System;
using System.Collections.Generic;
using System.Linq;

namespace ab
{
    public class Scoring
    {
        public static Lazy<Func<Experiment, IOrderedEnumerable<KeyValuePair<int, double>>>> Default = new Lazy<Func<Experiment, IOrderedEnumerable<KeyValuePair<int, double>>>>(HighestDistinctConvertingAlternative);

        private static Func<Experiment, IOrderedEnumerable<KeyValuePair<int, double>>> HighestDistinctConvertingAlternative()
        {
            return experiment => experiment.ConversionRateByAlternative().OrderBy(kv => kv.Value);
        }
    }
}