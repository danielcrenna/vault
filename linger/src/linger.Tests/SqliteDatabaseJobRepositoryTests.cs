using NUnit.Framework;
using tophat;

namespace linger.Tests
{
    [TestFixture]
    public class SqliteDatabaseJobRepositoryTests : SqliteFixture
    {
        private readonly DatabaseJobRepositoryTests _repositoryTests;

        public SqliteDatabaseJobRepositoryTests()
        {
            _repositoryTests = new DatabaseJobRepositoryTests(CreateRepository());
        }

        [Test]
        public void Saves_job()
        {
            _repositoryTests.Saves_job();
        }

        [Test]
        public void Updates_job()
        {
            _repositoryTests.Updates_job();
        }

        [Test]
        public void Loads_job()
        {
            _repositoryTests.Loads_job();
        }

        [Test]
        public void Fetches_and_locks_job()
        {
            _repositoryTests.Fetches_and_locks_job();
        }

        [Test]
        public void Fetches_all_jobs()
        {
            _repositoryTests.Fetches_all_jobs();
        }

        [Test]
        public void Creates_a_batch()
        {
            _repositoryTests.Creates_a_batch();
        }

        [Test]
        public void Jobs_in_batches_arent_fetched()
        {
            _repositoryTests.Jobs_in_batches_arent_fetched();
        }

        public DatabaseJobRepository CreateRepository()
        {
            return new DatabaseJobRepository(DatabaseDialect.Sqlite, () => UnitOfWork.Current);
        }
    }
}
