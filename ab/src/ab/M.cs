using System;

namespace ab
{
    public class M 
    {
        private static readonly MetricRepository MetricRepository;
        static M()
        {
            MetricRepository = new MetricRepository();
        }
        public static void Track(string metric, long increment = 1)
        {
            IMetric tracker;
            if(increment <= 0 || (tracker = MetricRepository.GetByName(metric)) == null)
            {
                return;
            }
            tracker.OnHook(new MetricEventArgs(metric, DateTime.Today.ToUnixTime(), increment));
        }
    }
}