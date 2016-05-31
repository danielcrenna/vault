using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ab
{
    public class ExperimentRepository : IExperimentRepository
    {
        private static readonly ConcurrentDictionary<string, Experiment> Inner = new ConcurrentDictionary<string, Experiment>();
        
        public void Save(Experiment experiment)
        {
            var key = experiment.Name;
            Inner.AddOrUpdate(key, experiment, (n, m) => m);
        }

        public IEnumerable<Experiment> GetAll()
        {
            return Inner.Values;
        }

        public IEnumerable<Experiment> GetByMetric(string metric)
        {
            foreach (var item in Inner.Values)
            {
                if (item.HasMetric(metric))
                {
                    yield return item;
                }
            }
        }

        public Experiment GetByName(string experiment)
        {
            foreach (var item in Inner.Values)
            {
                if (item.Name == experiment)
                {
                    return item;
                }
            }
            return null;
        }
    }
}