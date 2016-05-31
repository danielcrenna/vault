using System;
using System.Linq;
using NUnit.Framework;
using tophat;

namespace ab.Tests
{
    [TestFixture]
    public class SqlExperimentRepositoryTests : SqlServerFixture
    {
        [Test]
        public void Saves_experiment()
        {
            Assert.DoesNotThrow(() =>
            {
                var experiment = new Experiment("This is a test", "This is a test description");
                var repository = new SqlExperimentRepository(() => UnitOfWork.Current);
                repository.Save(experiment);
            });
        }

        [Test]
        public void Upserts_experiment()
        {
            Assert.DoesNotThrow(() =>
            {
                var experiment = new Experiment("This is a test", "This is a test description");
                var repository = new SqlExperimentRepository(() => UnitOfWork.Current);
                repository.Save(experiment);

                experiment = new Experiment(1, "This is a test", 1, experiment.CreatedAt, DateTime.Now);
                repository.Save(experiment);

                var experiments = repository.GetAll();
                Assert.AreEqual(1, experiments.Count());
            });
        }
    }
}