using System;
using System.Data;
using NUnit.Framework;

namespace tophat.Tests
{
    [TestFixture]
    public class UnitOfWorkTests : DatabaseFixture
    {
        [Test]
        public void Always_new_scoping_works_as_advertised()
        {
            var cs = CreateConnectionString();
            Database.Install<SqliteConnectionFactory>(cs, ConnectionScope.AlwaysNew);

            var connection1 = UnitOfWork.Current;
            var connection2 = UnitOfWork.Current;
            Assert.AreNotEqual(connection1, connection2);
        }

        [Test]
        public void Resolves_custom_scoping()
        {
            var cs = CreateConnectionString();
            Database.Install<SqliteConnectionFactory>(cs, Scope);

            var db = UnitOfWork.Current;
            Assert.IsNotNull(db);
            Assert.AreEqual(_foreverContext, Database.Container.Resolve<DataContext>());
        }

        [Test]
        public void Manages_borrowed_connection()
        {
            Func<string, IDbConnection> func = cs =>
            {
                var factory = new SqliteConnectionFactory { ConnectionString = cs };
                return factory.CreateConnection();
            };

            Database.Install(CreateConnectionString(), func);
            var db1 = UnitOfWork.Current;
            var db2 = UnitOfWork.Current;

            Assert.IsNotNull(db1);
            Assert.IsNotNull(db2);
            Assert.AreEqual(db1, db2);
        }
        
        private static DataContext _foreverContext;
        private static DataContext Scope(IConnectionFactory connectionFactory)
        {
            return _foreverContext ?? (_foreverContext = new DataContext(connectionFactory));
        }
    }
}
