using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Paging.Tests
{
    [TestFixture]
    public class SqlBuilderTests
    {
        [TestCase("SELECT * FROM [User] WHERE IsActivated = 0 AND EndDate IS NULL ORDER BY SignedInDate", SqlDialect.SqlServer)]
        [TestCase("SELECT DISTINCT Email FROM [User] WHERE IsActivated = 0 AND EndDate IS NULL ORDER BY SignedInDate", SqlDialect.SqlServer)]
        public void Can_build_sql(string sql, SqlDialect dialect)
        {
            PagingQuery query = null;
            Timed(() =>
            {
                query = SqlBuilder.Page(sql, 0, 10);
            });

            Assert.IsNotNull(query);
            Assert.IsNotNullOrEmpty(query.CountQuery, "Count query must exist");
            Assert.IsNotNullOrEmpty(query.PageQuery, "Page query must exist");
            Assert.IsNotEmpty(query.Parameters);

            Console.WriteLine(query.CountQuery);
            Console.WriteLine(query.PageQuery);
            foreach(var parameter in query.Parameters)
            {
                Console.WriteLine("{0} = {1}", parameter.Key, parameter.Value);
            }
        }

        private static void Timed(Action closure)
        {
            var sw = Stopwatch.StartNew();
            closure();
            Console.WriteLine(sw.Elapsed);
        }
    }
}
