using System.Collections.Generic;

namespace ab
{
    public interface IExperimentRepository
    {
        void Save(Experiment experiment);
        IEnumerable<Experiment> GetAll();
        IEnumerable<Experiment> GetByMetric(string metric);
        Experiment GetByName(string experiment);
    }
}