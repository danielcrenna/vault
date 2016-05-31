using System;
using System.Linq;
using NUnit.Framework;

namespace linger.Tests
{
    public class DatabaseJobRepositoryTests
    {
        private readonly DatabaseJobRepository _repository;

        public DatabaseJobRepositoryTests(DatabaseJobRepository repository)
        {
            _repository = repository;
        }

        public void Saves_job()
        {
            Assert.DoesNotThrow(() => SaveJob());
        }

        public void Updates_job()
        {
            Assert.DoesNotThrow(() =>
            {
                var job = SaveJob();
                job.SucceededAt = DateTime.Now;
                job.LastError = "Too good of shape";
                _repository.Save(job);
            });
        }

        public void Loads_job()
        {
            Assert.DoesNotThrow(() =>
            {
                var scheduled = SaveJob();
                var loaded = _repository.Load(scheduled.Id);
                Assert.IsNotNull(loaded);
            });
        }

        public void Fetches_and_locks_job()
        {
            var scheduled = SaveJob();
            var fetched = _repository.GetNextAvailable(1);

            Assert.AreEqual(1, fetched.Count);
            Assert.AreEqual(scheduled.Id, fetched[0].Id);
            Assert.IsNotNull(fetched[0].LockedAt, "Did not set lock date");
            Assert.IsNotNull(fetched[0].LockedBy, "Did not set lock user");
        }

        public void Fetches_all_jobs()
        {
            Assert.DoesNotThrow(() =>
            {
                var scheduled = SaveJob();
                var loaded = _repository.GetAll();
                Assert.IsNotNull(loaded);
                Assert.AreEqual(1, loaded.Count);
                Assert.AreEqual(scheduled, loaded.First());
            });
        }

        public void Creates_a_batch()
        {
            Assert.DoesNotThrow(() =>
            {
                var scheduled = SaveJob();
                var batch = _repository.CreateBatch("test", new []{ scheduled });

                Assert.IsNotNull(batch);
                Assert.IsNotNull(batch.Jobs);
                Assert.AreEqual("test", batch.Name);
                Assert.AreEqual(1, batch.Id);
                Assert.AreEqual(1, batch.Jobs.Count());
                Assert.AreEqual(scheduled, batch.Jobs.First());
            });
        }
        
        public void Jobs_in_batches_arent_fetched()
        {
            Assert.DoesNotThrow(() =>
            {
                var scheduled1 = Linger.CreateScheduledJob(new HelloWorldJob(), 0, null);
                _repository.Save(scheduled1);

                var scheduled2 = Linger.CreateScheduledJob(new HelloWorldJob(), 0, null);
                _repository.Save(scheduled2);

                var batch = _repository.CreateBatch("test", new[] {scheduled2});
                Assert.IsNotNull(batch);
                Assert.IsTrue(batch.Jobs.Contains(scheduled2));

                var all = _repository.GetAll();
                Assert.IsNotNull(all);
                Assert.AreEqual(1, all.Count);

                var available = _repository.GetNextAvailable(2);
                Assert.IsNotNull(available);
                Assert.AreEqual(1, available.Count);
            });
        }

        private ScheduledJob SaveJob()
        {
            var job = new HelloWorldJob();
            var scheduled = Linger.CreateScheduledJob(job, 0, null);
            _repository.Save(scheduled);
            Assert.AreEqual(1, scheduled.Id);
            return scheduled;
        }
    }
}