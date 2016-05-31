using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using tuxedo.Dapper;

namespace ab
{
    public class SqlExperimentRepository : IExperimentRepository 
    {
        private readonly Func<IDbConnection> _connector;

        public SqlExperimentRepository(Func<IDbConnection> connector)
        {
            _connector = connector;
        }

        public void Save(Experiment experiment)
        {
            var exists = GetByName(experiment.Name);
            var db = _connector();
            if(exists == null)
            {
                db.Insert(experiment);
            }
            else
            {
                db.Update(experiment);
            }
        }

        public IEnumerable<Experiment> GetAll()
        {
            return _connector().Query<Experiment>("SELECT * FROM Experiment");
        }

        public Experiment GetByName(string experiment)
        {
            return _connector().Query<Experiment>("SELECT * FROM Experiment WHERE Name = @Name", new { Name = experiment }).FirstOrDefault();
        }

        public IEnumerable<Experiment> GetByMetric(string metric)
        {
            throw new System.NotImplementedException();
        }
    }
}